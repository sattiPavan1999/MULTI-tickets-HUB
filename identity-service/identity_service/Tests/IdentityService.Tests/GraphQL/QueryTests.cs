using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityService.Core.DTOs;
using IdentityService.Core.Services;
using IdentityService.Endpoints.GraphQL;
using Moq;

namespace IdentityService.Tests.GraphQL;

public class QueryTests
{
    private static UserType MakeUser(int id = 1) => new()
    {
        Id = id,
        Email = "user@example.com",
        FullName = "John Doe",
        PhoneNumber = "+1234567890",
        Role = "User",
        CreatedAt = DateTime.UtcNow
    };

    private static ClaimsPrincipal MakePrincipal(string? nameId = "1", string? sub = null)
    {
        var claims = new List<Claim>();
        if (nameId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, nameId));
        if (sub != null) claims.Add(new Claim(JwtRegisteredClaimNames.Sub, sub));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    // ── GetMe ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_ValidNameIdentifierClaim_ReturnsUser()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(MakeUser());

        var result = await new Query().GetMe(MakePrincipal(nameId: "1"), svc.Object);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task GetMe_FallsBackToSubClaim_WhenNoNameIdentifier()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserByIdAsync(2)).ReturnsAsync(MakeUser(2));

        var principal = MakePrincipal(nameId: null, sub: "2");
        var result = await new Query().GetMe(principal, svc.Object);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
    }

    [Fact]
    public async Task GetMe_NoClaims_ThrowsUnauthorizedAccessException()
    {
        var svc = new Mock<IAuthService>();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new Query().GetMe(principal, svc.Object));
    }

    [Fact]
    public async Task GetMe_NonNumericClaim_ThrowsUnauthorizedAccessException()
    {
        var svc = new Mock<IAuthService>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new Query().GetMe(MakePrincipal(nameId: "not-a-number"), svc.Object));
    }

    [Fact]
    public async Task GetMe_UserNotFound_ThrowsInvalidOperationException()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((UserType?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new Query().GetMe(MakePrincipal("99"), svc.Object));
    }

    [Fact]
    public async Task GetMe_CallsAuthService_WithParsedUserId()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserByIdAsync(5)).ReturnsAsync(MakeUser(5));

        await new Query().GetMe(MakePrincipal("5"), svc.Object);

        svc.Verify(s => s.GetUserByIdAsync(5), Times.Once);
    }

    // ── GetUser ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUser_ExistingId_ReturnsUser()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(MakeUser());

        var result = await new Query().GetUser(1, svc.Object);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetUser_NonExistentId_ReturnsNull()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserByIdAsync(999)).ReturnsAsync((UserType?)null);

        var result = await new Query().GetUser(999, svc.Object);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUser_CallsAuthService_WithCorrectId()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserByIdAsync(7)).ReturnsAsync(MakeUser(7));

        await new Query().GetUser(7, svc.Object);

        svc.Verify(s => s.GetUserByIdAsync(7), Times.Once);
    }

    // ── GetUsers ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        var users = new List<UserType> { MakeUser(1), MakeUser(2) };
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

        var result = await new Query().GetUsers(svc.Object);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetUsers_EmptyDatabase_ReturnsEmptyList()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetAllUsersAsync()).ReturnsAsync([]);

        var result = await new Query().GetUsers(svc.Object);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUsers_CallsAuthService_Once()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetAllUsersAsync()).ReturnsAsync([]);

        await new Query().GetUsers(svc.Object);

        svc.Verify(s => s.GetAllUsersAsync(), Times.Once);
    }

    // ── GetUserCount ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserCount_ReturnsCountFromService()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserCountAsync()).ReturnsAsync(42);

        var result = await new Query().GetUserCount(svc.Object);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetUserCount_EmptyDatabase_ReturnsZero()
    {
        var svc = new Mock<IAuthService>();
        svc.Setup(s => s.GetUserCountAsync()).ReturnsAsync(0);

        var result = await new Query().GetUserCount(svc.Object);

        Assert.Equal(0, result);
    }
}
