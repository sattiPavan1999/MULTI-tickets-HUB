using AutoMapper;
using Bogus;
using IdentityService.Core.Data;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Mapping;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;
using IdentityService.Core.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IdentityService.Tests.Services;

/// <summary>
/// Service-layer tests — use EF InMemory for workflows that need the DB (UpdateProfile,
/// ForgotPassword, ResetPassword) and Moq for lightweight unit coverage of Register/Login/Get*.
/// </summary>
public class AuthServiceTests
{
    // ── Fakers ────────────────────────────────────────────────────────────────

    private static readonly Faker Fake = new();

    private static readonly Faker<RegisterInput> RegisterFaker = new Faker<RegisterInput>()
        .CustomInstantiator(f => new RegisterInput
        {
            Email = f.Internet.Email(),
            Password = "Password1!",
            FullName = f.Name.FullName(),
            PhoneNumber = f.Phone.PhoneNumber("+1##########")
        });

    private static readonly Faker<LoginInput> LoginFaker = new Faker<LoginInput>()
        .CustomInstantiator(f => new LoginInput
        {
            Email = f.Internet.Email(),
            Password = "Password1!"
        });

    // ── InMemory wiring ───────────────────────────────────────────────────────

    private static readonly IConfiguration DevConfig = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["PasswordReset:TokenExpiryMinutes"] = "30"
        })
        .Build();

    private static IMapper BuildMapper()
        => new MapperConfiguration(c => c.AddProfile<UserMappingProfile>()).CreateMapper();

    private static (IAuthService svc, IdentityDbContext db) BuildFullService(string dbName)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new IdentityDbContext(options);

        var mapper = BuildMapper();
        var userRepo = new UserRepository(db, NullLogger<UserRepository>.Instance);
        var resetRepo = new PasswordResetTokenRepository(db, NullLogger<PasswordResetTokenRepository>.Instance);
        var jwt = new StubJwtService();
        var audit = new Mock<IAuditService>().Object;

        var authSvc = new AuthenticationService(
            userRepo, jwt, audit,
            new RegisterInputValidator(), new LoginInputValidator(),
            mapper, NullLogger<AuthenticationService>.Instance);

        var accountSvc = new UserAccountService(
            userRepo, audit,
            new UpdateProfileInputValidator(),
            mapper, NullLogger<UserAccountService>.Instance);

        var pwdSvc = new PasswordService(
            userRepo, resetRepo, audit,
            DevConfig, NullLogger<PasswordService>.Instance);

        return (new AuthService(authSvc, accountSvc, pwdSvc), db);
    }

    private static async Task<User> SeedUserAsync(IdentityDbContext db,
        string? email = null, string password = "OldPassword1!")
    {
        var user = new User
        {
            Email = email ?? Fake.Internet.Email(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = Fake.Name.FullName(),
            PhoneNumber = Fake.Phone.PhoneNumber("+1##########"),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    // ── Mocked wiring ─────────────────────────────────────────────────────────

    private static User MakeEntity(string? email = null) => new()
    {
        Id = Fake.Random.Int(1, 1000),
        Email = email ?? Fake.Internet.Email(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
        FullName = Fake.Name.FullName(),
        PhoneNumber = Fake.Phone.PhoneNumber("+1##########"),
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };

    private static IAuthService BuildMocked(
        Mock<IUserRepository> userRepo,
        Mock<IPasswordResetTokenRepository>? resetRepo = null,
        Mock<IJwtService>? jwt = null)
    {
        var mapper = BuildMapper();
        var mockJwt = jwt ?? new Mock<IJwtService>();
        if (jwt is null) mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("stub-token");

        var authSvc = new AuthenticationService(
            userRepo.Object, mockJwt.Object, new Mock<IAuditService>().Object,
            new RegisterInputValidator(), new LoginInputValidator(),
            mapper, NullLogger<AuthenticationService>.Instance);

        var accountSvc = new UserAccountService(
            userRepo.Object, new Mock<IAuditService>().Object,
            new UpdateProfileInputValidator(),
            mapper, NullLogger<UserAccountService>.Instance);

        var mockReset = resetRepo ?? new Mock<IPasswordResetTokenRepository>();
        var pwdSvc = new PasswordService(
            userRepo.Object, mockReset.Object, new Mock<IAuditService>().Object,
            DevConfig, NullLogger<PasswordService>.Instance);

        return new AuthService(authSvc, accountSvc, pwdSvc);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_NewEmail_ReturnsUserType()
    {
        var input = RegisterFaker.Generate();
        var entity = MakeEntity(input.Email);
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var svc = BuildMocked(userRepo);

        var result = await svc.RegisterAsync(input);

        result.Email.Should().Be(input.Email);
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflictException()
    {
        var input = RegisterFaker.Generate();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync(input.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var svc = BuildMocked(userRepo);

        await svc.Invoking(s => s.RegisterAsync(input))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Register_HashesPassword_BeforeStoring()
    {
        var input = RegisterFaker.Generate();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        User? captured = null;
        userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => captured = u)
            .ReturnsAsync(MakeEntity());
        var svc = BuildMocked(userRepo);

        await svc.RegisterAsync(input);

        captured.Should().NotBeNull();
        captured!.PasswordHash.Should().NotBe(input.Password);
        BCrypt.Net.BCrypt.Verify(input.Password, captured.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_EmptyEmail_ThrowsValidationException()
    {
        var input = new RegisterInput { Email = "", Password = "Password1!", FullName = "Test", PhoneNumber = "+1234567890" };
        var userRepo = new Mock<IUserRepository>();
        var svc = BuildMocked(userRepo);

        await svc.Invoking(s => s.RegisterAsync(input))
            .Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    // ── Login ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndUser()
    {
        var entity = MakeEntity();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync(entity.Email, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var svc = BuildMocked(userRepo);

        var result = await svc.LoginAsync(new LoginInput { Email = entity.Email, Password = "Password1!" });

        result.Token.Should().Be("stub-token");
        result.User.Email.Should().Be(entity.Email);
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsUnauthorizedException()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var svc = BuildMocked(userRepo);

        await svc.Invoking(s => s.LoginAsync(new LoginInput { Email = "nobody@example.com", Password = "Password1!" }))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedException()
    {
        var entity = MakeEntity();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByEmailAsync(entity.Email, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var svc = BuildMocked(userRepo);

        await svc.Invoking(s => s.LoginAsync(new LoginInput { Email = entity.Email, Password = "WrongPassword!" }))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── GetUserById ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsUserType()
    {
        var entity = MakeEntity();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var svc = BuildMocked(userRepo);

        var result = await svc.GetUserByIdAsync(entity.Id);

        result.Should().NotBeNull();
        result!.Email.Should().Be(entity.Email);
    }

    [Fact]
    public async Task GetUserById_NonExistentUser_ReturnsNull()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var svc = BuildMocked(userRepo);

        var result = await svc.GetUserByIdAsync(999);

        result.Should().BeNull();
    }

    // ── GetAllUsers / GetUserCount ─────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsers_ReturnsListOfUserTypes()
    {
        var entities = Enumerable.Range(1, 3).Select(_ => MakeEntity()).ToList();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
        var svc = BuildMocked(userRepo);

        var result = await svc.GetAllUsersAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetUserCount_ReturnsCountFromRepository()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(7);
        var svc = BuildMocked(userRepo);

        var result = await svc.GetUserCountAsync();

        result.Should().Be(7);
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_UpdatesFullNameAndPhone()
    {
        var (svc, db) = BuildFullService(nameof(UpdateProfile_UpdatesFullNameAndPhone));
        var user = await SeedUserAsync(db);
        var newName = Fake.Name.FullName();

        var result = await svc.UpdateProfileAsync(user.Id, new UpdateProfileInput { FullName = newName });

        result.FullName.Should().Be(newName);
    }

    [Fact]
    public async Task UpdateProfile_ChangesEmail_WhenAvailable()
    {
        var (svc, db) = BuildFullService(nameof(UpdateProfile_ChangesEmail_WhenAvailable));
        var user = await SeedUserAsync(db);
        var newEmail = Fake.Internet.Email();

        var result = await svc.UpdateProfileAsync(user.Id, new UpdateProfileInput { Email = newEmail });

        result.Email.Should().Be(newEmail);
    }

    [Fact]
    public async Task UpdateProfile_RejectsEmail_AlreadyTaken()
    {
        var (svc, db) = BuildFullService(nameof(UpdateProfile_RejectsEmail_AlreadyTaken));
        var existing = await SeedUserAsync(db);
        var target = await SeedUserAsync(db);

        await svc.Invoking(s => s.UpdateProfileAsync(target.Id, new UpdateProfileInput { Email = existing.Email }))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateProfile_UnknownUser_ThrowsNotFoundException()
    {
        var (svc, _) = BuildFullService(nameof(UpdateProfile_UnknownUser_ThrowsNotFoundException));

        await svc.Invoking(s => s.UpdateProfileAsync(9999, new UpdateProfileInput { FullName = "X" }))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ── ForgotPassword ────────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_KnownUser_IssuesToken()
    {
        var (svc, db) = BuildFullService(nameof(ForgotPassword_KnownUser_IssuesToken));
        var user = await SeedUserAsync(db);

        var response = await svc.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });

        response.ResetToken.Should().NotBeNull();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        db.PasswordResetTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task ForgotPassword_UnknownUser_DoesNotLeakExistence()
    {
        var (svc, db) = BuildFullService(nameof(ForgotPassword_UnknownUser_DoesNotLeakExistence));

        var response = await svc.ForgotPasswordAsync(new ForgotPasswordInput { Email = "ghost@example.com" });

        response.ResetToken.Should().BeNull();
        db.PasswordResetTokens.Should().BeEmpty();
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_ValidToken_UpdatesPasswordAndConsumesToken()
    {
        var (svc, db) = BuildFullService(nameof(ResetPassword_ValidToken_UpdatesPasswordAndConsumesToken));
        var user = await SeedUserAsync(db);
        var forgot = await svc.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });

        var result = await svc.ResetPasswordAsync(new ResetPasswordInput
        {
            Token = forgot.ResetToken!,
            NewPassword = "BrandNewPass1!"
        });

        result.Success.Should().BeTrue();
        var refreshed = await db.Users.FindAsync(user.Id);
        BCrypt.Net.BCrypt.Verify("BrandNewPass1!", refreshed!.PasswordHash).Should().BeTrue();
        var token = await db.PasswordResetTokens.SingleAsync();
        token.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ThrowsUnauthorizedException()
    {
        var (svc, _) = BuildFullService(nameof(ResetPassword_InvalidToken_ThrowsUnauthorizedException));

        await svc.Invoking(s => s.ResetPasswordAsync(new ResetPasswordInput
        {
            Token = "not-a-real-token",
            NewPassword = "BrandNewPass1!"
        })).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ResetPassword_TokenCannotBeReused()
    {
        var (svc, db) = BuildFullService(nameof(ResetPassword_TokenCannotBeReused));
        var user = await SeedUserAsync(db);
        var forgot = await svc.ForgotPasswordAsync(new ForgotPasswordInput { Email = user.Email });

        await svc.ResetPasswordAsync(new ResetPasswordInput { Token = forgot.ResetToken!, NewPassword = "First1!" });

        await svc.Invoking(s => s.ResetPasswordAsync(new ResetPasswordInput
        {
            Token = forgot.ResetToken!,
            NewPassword = "Second1!"
        })).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class StubJwtService : IJwtService
    {
        public string GenerateToken(User user) => "stub-token";
        public bool ValidateToken(string token) => true;
    }
}
