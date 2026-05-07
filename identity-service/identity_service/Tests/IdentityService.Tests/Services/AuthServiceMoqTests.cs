using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IdentityService.Tests.Services;

/// <summary>
/// Moq-based unit tests for AuthService covering Register, Login, and
/// read operations (GetUserById, GetAllUsers, GetUserCount).
/// Mutation-heavy flows (ForgotPassword, ResetPassword, UpdateProfile)
/// are covered with real repos in AuthServiceTests.
/// </summary>
public class AuthServiceMoqTests
{
    private static readonly IConfiguration DevConfig = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["PasswordReset:TokenExpiryMinutes"] = "30"
        })
        .Build();

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

    private static AuthService BuildService(
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

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_NewEmail_ReturnsUserType()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync("user@example.com")).ReturnsAsync(false);
        userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(MakeUserEntity());
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo);

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
        var entity = MakeUserEntity();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(entity);
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo, jwt: jwt);

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
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo);

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
        var service = BuildService(userRepo);

        var result = await service.GetAllUsersAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.IsType<UserType>(u));
    }

    [Fact]
    public async Task GetAllUsers_EmptyDb_ReturnsEmptyList()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        var service = BuildService(userRepo);

        var result = await service.GetAllUsersAsync();

        Assert.Empty(result);
    }

    // ── GetUserCount ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserCount_ReturnsCountFromRepository()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.CountAsync()).ReturnsAsync(7);
        var service = BuildService(userRepo);

        var result = await service.GetUserCountAsync();

        Assert.Equal(7, result);
    }
}
