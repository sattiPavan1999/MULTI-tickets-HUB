using IdentityService.Core.Data;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IdentityService.Tests.Repositories;

public class PasswordResetTokenRepositoryTests
{
    private static (PasswordResetTokenRepository repo, IdentityDbContext db) Build(string dbName)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new IdentityDbContext(options);
        var repo = new PasswordResetTokenRepository(db, NullLogger<PasswordResetTokenRepository>.Instance);
        return (repo, db);
    }

    private static async Task<User> SeedUser(IdentityDbContext db, string email = "user@example.com")
    {
        var user = new User
        {
            Email = email,
            PasswordHash = "hash",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static PasswordResetToken NewToken(int userId, string hash = "abc123", int expiryMinutes = 30) => new()
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
        var (repo, db) = Build(nameof(CreateAsync_PersistsToken));
        var user = await SeedUser(db);

        await repo.CreateAsync(NewToken(user.Id));

        Assert.Equal(1, await db.PasswordResetTokens.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAt()
    {
        var (repo, db) = Build(nameof(CreateAsync_SetsCreatedAt));
        var user = await SeedUser(db);
        var before = DateTime.UtcNow;

        var created = await repo.CreateAsync(NewToken(user.Id));

        Assert.True(created.CreatedAt >= before);
    }

    [Fact]
    public async Task CreateAsync_ReturnsTokenWithId()
    {
        var (repo, db) = Build(nameof(CreateAsync_ReturnsTokenWithId));
        var user = await SeedUser(db);

        var created = await repo.CreateAsync(NewToken(user.Id));

        Assert.True(created.Id > 0);
    }

    // ── GetActiveByHashAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveByHashAsync_ValidToken_ReturnsToken()
    {
        var (repo, db) = Build(nameof(GetActiveByHashAsync_ValidToken_ReturnsToken));
        var user = await SeedUser(db);
        db.PasswordResetTokens.Add(NewToken(user.Id, "valid-hash"));
        await db.SaveChangesAsync();

        var result = await repo.GetActiveByHashAsync("valid-hash");

        Assert.NotNull(result);
        Assert.Equal("valid-hash", result!.TokenHash);
    }

    [Fact]
    public async Task GetActiveByHashAsync_WrongHash_ReturnsNull()
    {
        var (repo, db) = Build(nameof(GetActiveByHashAsync_WrongHash_ReturnsNull));
        var user = await SeedUser(db);
        db.PasswordResetTokens.Add(NewToken(user.Id, "correct-hash"));
        await db.SaveChangesAsync();

        var result = await repo.GetActiveByHashAsync("wrong-hash");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveByHashAsync_ExpiredToken_ReturnsNull()
    {
        var (repo, db) = Build(nameof(GetActiveByHashAsync_ExpiredToken_ReturnsNull));
        var user = await SeedUser(db);
        db.PasswordResetTokens.Add(NewToken(user.Id, "expired-hash", expiryMinutes: -1));
        await db.SaveChangesAsync();

        var result = await repo.GetActiveByHashAsync("expired-hash");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveByHashAsync_AlreadyUsedToken_ReturnsNull()
    {
        var (repo, db) = Build(nameof(GetActiveByHashAsync_AlreadyUsedToken_ReturnsNull));
        var user = await SeedUser(db);
        var token = NewToken(user.Id, "used-hash");
        token.UsedAt = DateTime.UtcNow.AddMinutes(-5);
        db.PasswordResetTokens.Add(token);
        await db.SaveChangesAsync();

        var result = await repo.GetActiveByHashAsync("used-hash");

        Assert.Null(result);
    }

    // ── MarkUsedAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkUsedAsync_SetsUsedAt()
    {
        var (repo, db) = Build(nameof(MarkUsedAsync_SetsUsedAt));
        var user = await SeedUser(db);
        var token = NewToken(user.Id, "mark-hash");
        db.PasswordResetTokens.Add(token);
        await db.SaveChangesAsync();
        var saved = await db.PasswordResetTokens.FirstAsync();
        var before = DateTime.UtcNow;

        await repo.MarkUsedAsync(saved);

        Assert.NotNull(saved.UsedAt);
        Assert.True(saved.UsedAt >= before);
    }

    [Fact]
    public async Task MarkUsedAsync_TokenBecomesInactive()
    {
        var (repo, db) = Build(nameof(MarkUsedAsync_TokenBecomesInactive));
        var user = await SeedUser(db);
        var token = NewToken(user.Id, "inactive-hash");
        db.PasswordResetTokens.Add(token);
        await db.SaveChangesAsync();
        var saved = await db.PasswordResetTokens.FirstAsync();

        await repo.MarkUsedAsync(saved);
        var result = await repo.GetActiveByHashAsync("inactive-hash");

        Assert.Null(result);
    }

    // ── InvalidateActiveForUserAsync ──────────────────────────────────────────

    [Fact]
    public async Task InvalidateActiveForUserAsync_MarksAllActiveTokensUsed()
    {
        var (repo, db) = Build(nameof(InvalidateActiveForUserAsync_MarksAllActiveTokensUsed));
        var user = await SeedUser(db);
        db.PasswordResetTokens.AddRange(
            NewToken(user.Id, "hash1"),
            NewToken(user.Id, "hash2")
        );
        await db.SaveChangesAsync();

        await repo.InvalidateActiveForUserAsync(user.Id);

        var tokens = await db.PasswordResetTokens.ToListAsync();
        Assert.All(tokens, t => Assert.NotNull(t.UsedAt));
    }

    [Fact]
    public async Task InvalidateActiveForUserAsync_LeavesExpiredTokensAlone()
    {
        var (repo, db) = Build(nameof(InvalidateActiveForUserAsync_LeavesExpiredTokensAlone));
        var user = await SeedUser(db);
        var expired = NewToken(user.Id, "expired", expiryMinutes: -10);
        db.PasswordResetTokens.Add(expired);
        await db.SaveChangesAsync();

        await repo.InvalidateActiveForUserAsync(user.Id);

        var token = await db.PasswordResetTokens.FirstAsync();
        Assert.Null(token.UsedAt);
    }

    [Fact]
    public async Task InvalidateActiveForUserAsync_DoesNotAffectOtherUsers()
    {
        var (repo, db) = Build(nameof(InvalidateActiveForUserAsync_DoesNotAffectOtherUsers));
        var userA = await SeedUser(db, "a@example.com");
        var userB = await SeedUser(db, "b@example.com");
        db.PasswordResetTokens.AddRange(
            NewToken(userA.Id, "hash-a"),
            NewToken(userB.Id, "hash-b")
        );
        await db.SaveChangesAsync();

        await repo.InvalidateActiveForUserAsync(userA.Id);

        var tokenB = await db.PasswordResetTokens.FirstAsync(t => t.UserId == userB.Id);
        Assert.Null(tokenB.UsedAt);
    }

    [Fact]
    public async Task InvalidateActiveForUserAsync_NoTokens_DoesNotThrow()
    {
        var (repo, _) = Build(nameof(InvalidateActiveForUserAsync_NoTokens_DoesNotThrow));

        var ex = await Record.ExceptionAsync(() => repo.InvalidateActiveForUserAsync(999));

        Assert.Null(ex);
    }
}
