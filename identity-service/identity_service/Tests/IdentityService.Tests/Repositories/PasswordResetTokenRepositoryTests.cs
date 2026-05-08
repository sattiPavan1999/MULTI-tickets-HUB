using Bogus;
using IdentityService.Core.Data;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IdentityService.Tests.Repositories;

[Collection("postgres")]
public class PasswordResetTokenRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private IdentityDbContext _context = null!;
    private PasswordResetTokenRepository _repo = null!;
    private UserRepository _userRepo = null!;

    private static readonly Faker<User> UserFaker = new Faker<User>()
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.PasswordHash, _ => "hash")
        .RuleFor(u => u.FullName, f => f.Name.FullName())
        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("+1##########"))
        .RuleFor(u => u.Role, _ => "User")
        .RuleFor(u => u.CreatedAt, _ => DateTime.UtcNow);

    public PasswordResetTokenRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        _context = new IdentityDbContext(options);

        // Clean data only — migrations already applied by the fixture (tokens first due to FK)
        _context.PasswordResetTokens.RemoveRange(_context.PasswordResetTokens);
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        _userRepo = new UserRepository(_context, NullLogger<UserRepository>.Instance);
        _repo = new PasswordResetTokenRepository(_context, NullLogger<PasswordResetTokenRepository>.Instance);
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    private async Task<User> SeedUserAsync()
    {
        var user = UserFaker.Generate();
        return await _userRepo.AddAsync(user);
    }

    private static PasswordResetToken NewToken(int userId, string hash, int expiryMinutes = 30) => new()
    {
        UserId = userId,
        TokenHash = hash,
        ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
        CreatedAt = DateTime.UtcNow
    };

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsToken()
    {
        var user = await SeedUserAsync();
        await _repo.CreateAsync(NewToken(user.Id, "abc123"));

        (await _context.PasswordResetTokens.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ReturnsTokenWithId()
    {
        var user = await SeedUserAsync();
        var created = await _repo.CreateAsync(NewToken(user.Id, "abc123"));

        created.Id.Should().BeGreaterThan(0);
    }

    // ── GetActiveByHashAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveByHashAsync_ValidToken_ReturnsToken()
    {
        var user = await SeedUserAsync();
        await _repo.CreateAsync(NewToken(user.Id, "valid-hash"));

        var result = await _repo.GetActiveByHashAsync("valid-hash");

        result.Should().NotBeNull();
        result!.TokenHash.Should().Be("valid-hash");
    }

    [Fact]
    public async Task GetActiveByHashAsync_WrongHash_ReturnsNull()
    {
        var user = await SeedUserAsync();
        await _repo.CreateAsync(NewToken(user.Id, "correct-hash"));

        var result = await _repo.GetActiveByHashAsync("wrong-hash");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByHashAsync_ExpiredToken_ReturnsNull()
    {
        var user = await SeedUserAsync();
        await _repo.CreateAsync(NewToken(user.Id, "expired-hash", expiryMinutes: -1));

        var result = await _repo.GetActiveByHashAsync("expired-hash");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByHashAsync_AlreadyUsedToken_ReturnsNull()
    {
        var user = await SeedUserAsync();
        var token = NewToken(user.Id, "used-hash");
        token.UsedAt = DateTime.UtcNow.AddMinutes(-5);
        await _repo.CreateAsync(token);

        var result = await _repo.GetActiveByHashAsync("used-hash");

        result.Should().BeNull();
    }

    // ── MarkUsedAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkUsedAsync_SetsUsedAt()
    {
        var user = await SeedUserAsync();
        var created = await _repo.CreateAsync(NewToken(user.Id, "mark-hash"));
        var before = DateTime.UtcNow;

        await _repo.MarkUsedAsync(created);

        created.UsedAt.Should().NotBeNull();
        created.UsedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task MarkUsedAsync_TokenBecomesInactive()
    {
        var user = await SeedUserAsync();
        var created = await _repo.CreateAsync(NewToken(user.Id, "inactive-hash"));

        await _repo.MarkUsedAsync(created);
        var result = await _repo.GetActiveByHashAsync("inactive-hash");

        result.Should().BeNull();
    }

    // ── InvalidateActiveForUserAsync ──────────────────────────────────────────

    [Fact]
    public async Task InvalidateActiveForUserAsync_MarksAllActiveTokensUsed()
    {
        var user = await SeedUserAsync();
        await _repo.CreateAsync(NewToken(user.Id, "hash1"));
        await _repo.CreateAsync(NewToken(user.Id, "hash2"));

        await _repo.InvalidateActiveForUserAsync(user.Id);

        var tokens = await _context.PasswordResetTokens.ToListAsync();
        tokens.Should().AllSatisfy(t => t.UsedAt.Should().NotBeNull());
    }

    [Fact]
    public async Task InvalidateActiveForUserAsync_DoesNotAffectOtherUsers()
    {
        var userA = await SeedUserAsync();
        var userB = await SeedUserAsync();
        await _repo.CreateAsync(NewToken(userA.Id, "hash-a"));
        await _repo.CreateAsync(NewToken(userB.Id, "hash-b"));

        await _repo.InvalidateActiveForUserAsync(userA.Id);

        var tokenB = await _context.PasswordResetTokens.FirstAsync(t => t.UserId == userB.Id);
        tokenB.UsedAt.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateActiveForUserAsync_NoTokens_DoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(() => _repo.InvalidateActiveForUserAsync(999));

        ex.Should().BeNull();
    }
}
