using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bogus;
using IdentityService.Core.DTOs;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;
using IdentityService.Endpoints.GraphQL;
using Moq;

namespace IdentityService.Tests.GraphQL;

public class QueryTests
{
    private static readonly Faker Fake = new();

    private static UserType MakeUserType(int id = 1) => new()
    {
        Id = id, Email = Fake.Internet.Email(), FullName = Fake.Name.FullName(),
        PhoneNumber = "+1234567890", Role = "User", CreatedAt = DateTime.UtcNow
    };

    private static User MakeUserEntity(int id = 1) => new()
    {
        Id = id, Email = Fake.Internet.Email(), PasswordHash = "hash",
        FullName = Fake.Name.FullName(), PhoneNumber = "+1234567890",
        Role = "User", CreatedAt = DateTime.UtcNow
    };

    private static ClaimsPrincipal MakePrincipal(string? nameId = "1", string? sub = null)
    {
        var claims = new List<Claim>();
        if (nameId != null) claims.Add(new(ClaimTypes.NameIdentifier, nameId));
        if (sub != null) claims.Add(new(JwtRegisteredClaimNames.Sub, sub));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    // ── GetMe ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_ValidNameIdentifierClaim_ReturnsUser()
    {
        var user = MakeUserType(1);
        var svc = new Mock<IUserAccountService>();
        svc.Setup(s => s.GetUserByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await new Query().GetMe(MakePrincipal("1"), svc.Object, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result!.Email);
    }

    [Fact]
    public async Task GetMe_FallsBackToSubClaim_WhenNoNameIdentifier()
    {
        var user = MakeUserType(2);
        var svc = new Mock<IUserAccountService>();
        svc.Setup(s => s.GetUserByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await new Query().GetMe(MakePrincipal(nameId: null, sub: "2"), svc.Object, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
    }

    [Fact]
    public async Task GetMe_NoClaims_ThrowsUnauthorizedAccessException()
    {
        var svc = new Mock<IUserAccountService>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new Query().GetMe(new ClaimsPrincipal(new ClaimsIdentity()), svc.Object, CancellationToken.None));
    }

    [Fact]
    public async Task GetMe_NonNumericClaim_ThrowsUnauthorizedAccessException()
    {
        var svc = new Mock<IUserAccountService>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new Query().GetMe(MakePrincipal("not-a-number"), svc.Object, CancellationToken.None));
    }

    [Fact]
    public async Task GetMe_UserNotFound_ThrowsInvalidOperationException()
    {
        var svc = new Mock<IUserAccountService>();
        svc.Setup(s => s.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserType?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new Query().GetMe(MakePrincipal("99"), svc.Object, CancellationToken.None));
    }

    // ── GetUser ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUser_ExistingId_ReturnsUser()
    {
        var user = MakeUserType(1);
        var svc = new Mock<IUserAccountService>();
        svc.Setup(s => s.GetUserByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await new Query().GetUser(1, svc.Object, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result!.Email);
    }

    [Fact]
    public async Task GetUser_NonExistentId_ReturnsNull()
    {
        var svc = new Mock<IUserAccountService>();
        svc.Setup(s => s.GetUserByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((UserType?)null);

        var result = await new Query().GetUser(999, svc.Object, CancellationToken.None);

        Assert.Null(result);
    }

    // ── GetUsers ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetUsers_ReturnsQueryableFromRepository()
    {
        var entities = Enumerable.Range(1, 3).Select(i => MakeUserEntity(i)).AsQueryable();
        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.Query()).Returns(entities);

        var result = new Query().GetUsers(repoMock.Object);

        Assert.Equal(3, result.Count());
        repoMock.Verify(r => r.Query(), Times.Once);
    }

    [Fact]
    public void GetUsers_EmptyRepository_ReturnsEmptyQueryable()
    {
        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.Query()).Returns(Enumerable.Empty<User>().AsQueryable());

        var result = new Query().GetUsers(repoMock.Object);

        Assert.Empty(result);
    }

    // ── GetUserCount ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserCount_ReturnsCountFromService()
    {
        var svc = new Mock<IUserAccountService>();
        svc.Setup(s => s.GetUserCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var result = await new Query().GetUserCount(svc.Object, CancellationToken.None);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetUserCount_EmptyDatabase_ReturnsZero()
    {
        var svc = new Mock<IUserAccountService>();
        svc.Setup(s => s.GetUserCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await new Query().GetUserCount(svc.Object, CancellationToken.None);

        Assert.Equal(0, result);
    }
}
