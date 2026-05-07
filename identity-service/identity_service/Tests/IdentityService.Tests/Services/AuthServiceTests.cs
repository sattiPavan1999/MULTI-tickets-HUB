using IdentityService.Core.Data;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IdentityService.Tests.Services;

public class AuthServiceTests
{
    private static (AuthService service, IdentityDbContext db) BuildService(
        string dbName,
        IConfiguration? configuration = null)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var db = new IdentityDbContext(options);

        var userRepo = new UserRepository(db, NullLogger<UserRepository>.Instance);
        var resetRepo = new PasswordResetTokenRepository(db, NullLogger<PasswordResetTokenRepository>.Instance);
        var jwt = new StubJwtService();
        var audit = new StubAuditService();
        var config = configuration ?? new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
            })
            .Build();

        var service = new AuthService(
            userRepo,
            resetRepo,
            jwt,
            audit,
            config,
            NullLogger<AuthService>.Instance);

        return (service, db);
    }

    private static async Task<User> SeedUserAsync(IdentityDbContext db, string email = "user@example.com")
    {
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword1!"),
            FullName = "Original Name",
            PhoneNumber = "+1234567890",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task UpdateProfile_UpdatesFullNameAndPhone()
    {
        var (service, db) = BuildService(nameof(UpdateProfile_UpdatesFullNameAndPhone));
        var user = await SeedUserAsync(db);

        var result = await service.UpdateProfileAsync(user.Id, new UpdateProfileInput
        {
            FullName = "New Name",
            PhoneNumber = "+19998887777"
        });

        Assert.Equal("New Name", result.FullName);
        Assert.Equal("+19998887777", result.PhoneNumber);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task UpdateProfile_ChangesEmail_WhenAvailable()
    {
        var (service, db) = BuildService(nameof(UpdateProfile_ChangesEmail_WhenAvailable));
        var user = await SeedUserAsync(db);

        var result = await service.UpdateProfileAsync(user.Id, new UpdateProfileInput
        {
            Email = "new@example.com"
        });

        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateProfile_RejectsEmail_AlreadyTaken()
    {
        var (service, db) = BuildService(nameof(UpdateProfile_RejectsEmail_AlreadyTaken));
        var user = await SeedUserAsync(db, "user@example.com");
        await SeedUserAsync(db, "taken@example.com");

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateProfileAsync(user.Id, new UpdateProfileInput
            {
                Email = "taken@example.com"
            }));
    }

    [Fact]
    public async Task UpdateProfile_UnknownUser_Throws()
    {
        var (service, _) = BuildService(nameof(UpdateProfile_UnknownUser_Throws));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateProfileAsync(9999, new UpdateProfileInput
            {
                FullName = "X"
            }));
    }

    [Fact]
    public async Task ForgotPassword_KnownUser_IssuesToken()
    {
        var (service, db) = BuildService(nameof(ForgotPassword_KnownUser_IssuesToken));
        var user = await SeedUserAsync(db);

        var response = await service.ForgotPasswordAsync(new ForgotPasswordInput
        {
            Email = user.Email
        });

        Assert.NotNull(response.ResetToken);
        Assert.NotNull(response.ExpiresAt);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
        Assert.Single(db.PasswordResetTokens);
    }

    [Fact]
    public async Task ForgotPassword_UnknownUser_DoesNotLeakExistence()
    {
        var (service, db) = BuildService(nameof(ForgotPassword_UnknownUser_DoesNotLeakExistence));

        var response = await service.ForgotPasswordAsync(new ForgotPasswordInput
        {
            Email = "missing@example.com"
        });

        Assert.Null(response.ResetToken);
        Assert.NotNull(response.Message);
        Assert.Empty(db.PasswordResetTokens);
    }

    [Fact]
    public async Task ForgotPassword_InvalidatesPreviousActiveTokens()
    {
        var (service, db) = BuildService(nameof(ForgotPassword_InvalidatesPreviousActiveTokens));
        var user = await SeedUserAsync(db);

        await service.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });
        await service.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });

        var tokens = await db.PasswordResetTokens.ToListAsync();
        Assert.Equal(2, tokens.Count);
        Assert.Single(tokens, t => t.UsedAt == null);
    }

    [Fact]
    public async Task ResetPassword_ValidToken_UpdatesPasswordAndConsumesToken()
    {
        var (service, db) = BuildService(nameof(ResetPassword_ValidToken_UpdatesPasswordAndConsumesToken));
        var user = await SeedUserAsync(db);

        var forgot = await service.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });

        var result = await service.ResetPasswordAsync(new ResetPasswordInput
        {
            Token = forgot.ResetToken!,
            NewPassword = "BrandNewPass1!"
        });

        Assert.True(result.Success);

        var refreshed = await db.Users.FindAsync(user.Id);
        Assert.NotNull(refreshed);
        Assert.True(BCrypt.Net.BCrypt.Verify("BrandNewPass1!", refreshed!.PasswordHash));

        var token = await db.PasswordResetTokens.SingleAsync();
        Assert.NotNull(token.UsedAt);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_Throws()
    {
        var (service, db) = BuildService(nameof(ResetPassword_InvalidToken_Throws));
        await SeedUserAsync(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ResetPasswordAsync(new ResetPasswordInput
            {
                Token = "not-a-real-token",
                NewPassword = "BrandNewPass1!"
            }));
    }

    [Fact]
    public async Task ResetPassword_TokenCannotBeReused()
    {
        var (service, db) = BuildService(nameof(ResetPassword_TokenCannotBeReused));
        var user = await SeedUserAsync(db);

        var forgot = await service.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });

        await service.ResetPasswordAsync(new ResetPasswordInput
        {
            Token = forgot.ResetToken!,
            NewPassword = "FirstReset1!"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ResetPasswordAsync(new ResetPasswordInput
            {
                Token = forgot.ResetToken!,
                NewPassword = "SecondReset1!"
            }));
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_Throws()
    {
        var (service, db) = BuildService(nameof(ResetPassword_ExpiredToken_Throws));
        var user = await SeedUserAsync(db);

        var forgot = await service.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });

        var token = await db.PasswordResetTokens.SingleAsync();
        token.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ResetPasswordAsync(new ResetPasswordInput
            {
                Token = forgot.ResetToken!,
                NewPassword = "BrandNewPass1!"
            }));
    }

    private sealed class StubJwtService : IJwtService
    {
        public string GenerateToken(User user) => "stub-token";
        public bool ValidateToken(string token) => true;
    }

    private sealed class StubAuditService : IAuditService
    {
        public Task LogAsync(string message) => Task.CompletedTask;
    }
}
