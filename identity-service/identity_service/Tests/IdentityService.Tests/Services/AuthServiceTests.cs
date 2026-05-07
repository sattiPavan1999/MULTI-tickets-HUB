using IdentityService.Core.Data;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IdentityService.Tests.Services;

public class AuthServiceTests
{
    // ── Helpers: real in-memory repos (used for UpdateProfile, ForgotPassword, ResetPassword) ──

    private static readonly IConfiguration DevConfig = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["PasswordReset:TokenExpiryMinutes"] = "30"
        })
        .Build();

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

        var service = new AuthService(
            userRepo,
            resetRepo,
            jwt,
            audit,
            configuration ?? DevConfig,
            NullLogger<AuthService>.Instance);

        return (service, db);
    }

    // ── Helpers: mocked repos (used for Register, Login, Get* operations) ──

    private static UserType MakeUserType(int id = 1) => new()
    {
        Id = id,
        Email = "user@example.com",
        FullName = "John Doe",
        PhoneNumber = "+1234567890",
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };

    private static User MakeUserEntity(int id = 1) => new()
    {
        Id = id,
        Email = "user@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
        FullName = "John Doe",
        PhoneNumber = "+1234567890",
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };

    private static AuthService BuildMockedService(
        Mock<IUserRepository> userRepo,
        Mock<IPasswordResetTokenRepository>? resetRepo = null,
        Mock<IJwtService>? jwt = null,
        IConfiguration? config = null)
    {
        var mockResetRepo = resetRepo ?? new Mock<IPasswordResetTokenRepository>();
        var mockJwt = jwt ?? new Mock<IJwtService>();
        if (jwt == null)
            mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("stub-token");

        return new AuthService(
            userRepo.Object,
            mockResetRepo.Object,
            mockJwt.Object,
            new Mock<IAuditService>().Object,
            config ?? DevConfig,
            NullLogger<AuthService>.Instance);
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

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_NewEmail_ReturnsUserType()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
        userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(MakeUserEntity());
        var service = BuildMockedService(userRepo);

        var result = await service.RegisterAsync(new RegisterInput
        {
            Email = "user@example.com",
            Password = "Password1!",
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
        });

        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflictException()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync("user@example.com")).ReturnsAsync(true);
        var service = BuildMockedService(userRepo);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(new RegisterInput
            {
                Email = "user@example.com",
                Password = "Password1!",
                FullName = "John Doe",
                PhoneNumber = "+1234567890"
            }));
    }

    [Fact]
    public async Task Register_HashesPassword_BeforeStoring()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        User? capturedUser = null;
        userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync(MakeUserEntity());
        var service = BuildMockedService(userRepo);

        await service.RegisterAsync(new RegisterInput
        {
            Email = "user@example.com",
            Password = "Password1!",
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
        });

        Assert.NotNull(capturedUser);
        Assert.NotEqual("Password1!", capturedUser!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password1!", capturedUser.PasswordHash));
    }

    [Fact]
    public async Task Register_DoesNotCallCreate_WhenEmailExists()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        var service = BuildMockedService(userRepo);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(new RegisterInput
            {
                Email = "user@example.com",
                Password = "Password1!",
                FullName = "J",
                PhoneNumber = "+1"
            }));

        userRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    // ── Login ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndUser()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(MakeUserEntity());
        var service = BuildMockedService(userRepo);

        var result = await service.LoginAsync(new LoginInput
        {
            Email = "user@example.com",
            Password = "Password1!"
        });

        Assert.Equal("stub-token", result.Token);
        Assert.Equal("user@example.com", result.User.Email);
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var service = BuildMockedService(userRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginInput
            {
                Email = "nobody@example.com",
                Password = "Password1!"
            }));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(MakeUserEntity());
        var service = BuildMockedService(userRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginInput
            {
                Email = "user@example.com",
                Password = "WrongPassword!"
            }));
    }

    [Fact]
    public async Task Login_GeneratesToken_ViaJwtService()
    {
        var entity = MakeUserEntity();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(entity);
        var jwt = new Mock<IJwtService>();
        jwt.Setup(j => j.GenerateToken(entity)).Returns("generated-token");
        var service = BuildMockedService(userRepo, jwt: jwt);

        var result = await service.LoginAsync(new LoginInput
        {
            Email = "user@example.com",
            Password = "Password1!"
        });

        Assert.Equal("generated-token", result.Token);
        jwt.Verify(j => j.GenerateToken(entity), Times.Once);
    }

    // ── GetUserById ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsUserType()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeUserEntity());
        var service = BuildMockedService(userRepo);

        var result = await service.GetUserByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserById_NonExistentUser_ReturnsNull()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);
        var service = BuildMockedService(userRepo);

        var result = await service.GetUserByIdAsync(999);

        Assert.Null(result);
    }

    // ── GetAllUsers ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsers_ReturnsListOfUserTypes()
    {
        var users = new List<User> { MakeUserEntity(1), MakeUserEntity(2) };
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
        var service = BuildMockedService(userRepo);

        var result = await service.GetAllUsersAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.IsType<UserType>(u));
    }

    [Fact]
    public async Task GetAllUsers_EmptyDb_ReturnsEmptyList()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        var service = BuildMockedService(userRepo);

        var result = await service.GetAllUsersAsync();

        Assert.Empty(result);
    }

    // ── GetUserCount ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserCount_ReturnsCountFromRepository()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.CountAsync()).ReturnsAsync(7);
        var service = BuildMockedService(userRepo);

        var result = await service.GetUserCountAsync();

        Assert.Equal(7, result);
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

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

    // ── ForgotPassword ────────────────────────────────────────────────────────

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

    // ── ResetPassword ─────────────────────────────────────────────────────────

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

    // ── Stubs ─────────────────────────────────────────────────────────────────

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
