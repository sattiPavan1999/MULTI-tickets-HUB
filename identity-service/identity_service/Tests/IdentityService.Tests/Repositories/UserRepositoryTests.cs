using IdentityService.Core.Data;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IdentityService.Tests.Repositories;

public class UserRepositoryTests
{
    private static (UserRepository repo, IdentityDbContext db) Build(string dbName)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new IdentityDbContext(options);
        var repo = new UserRepository(db, NullLogger<UserRepository>.Instance);
        return (repo, db);
    }

    private static User NewUser(string email = "user@example.com") => new()
    {
        Email = email,
        PasswordHash = "hashed",
        FullName = "John Doe",
        PhoneNumber = "+1234567890",
        Role = "User"
    };

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var (repo, db) = Build(nameof(GetByIdAsync_ExistingUser_ReturnsUser));
        db.Users.Add(NewUser());
        await db.SaveChangesAsync();
        var saved = db.Users.First();

        var result = await repo.GetByIdAsync(saved.Id);

        Assert.NotNull(result);
        Assert.Equal(saved.Id, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var (repo, _) = Build(nameof(GetByIdAsync_NonExistentId_ReturnsNull));

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    // ── GetByEmailAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        var (repo, db) = Build(nameof(GetByEmailAsync_ExistingEmail_ReturnsUser));
        db.Users.Add(NewUser("find@example.com"));
        await db.SaveChangesAsync();

        var result = await repo.GetByEmailAsync("find@example.com");

        Assert.NotNull(result);
        Assert.Equal("find@example.com", result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        var (repo, _) = Build(nameof(GetByEmailAsync_NonExistentEmail_ReturnsNull));

        var result = await repo.GetByEmailAsync("nobody@example.com");

        Assert.Null(result);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsUser_AndReturnsWithId()
    {
        var (repo, db) = Build(nameof(CreateAsync_PersistsUser_AndReturnsWithId));

        var created = await repo.CreateAsync(NewUser());

        Assert.True(created.Id > 0);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAt()
    {
        var (repo, _) = Build(nameof(CreateAsync_SetsCreatedAt));
        var before = DateTime.UtcNow;

        var created = await repo.CreateAsync(NewUser());

        Assert.True(created.CreatedAt >= before);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var (repo, db) = Build(nameof(UpdateAsync_PersistsChanges));
        db.Users.Add(NewUser());
        await db.SaveChangesAsync();
        var user = db.Users.First();

        user.FullName = "Updated Name";
        await repo.UpdateAsync(user);

        var refreshed = await db.Users.FindAsync(user.Id);
        Assert.Equal("Updated Name", refreshed!.FullName);
    }

    // ── EmailExistsAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        var (repo, db) = Build(nameof(EmailExistsAsync_ExistingEmail_ReturnsTrue));
        db.Users.Add(NewUser("exists@example.com"));
        await db.SaveChangesAsync();

        var result = await repo.EmailExistsAsync("exists@example.com");

        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
    {
        var (repo, _) = Build(nameof(EmailExistsAsync_NonExistentEmail_ReturnsFalse));

        var result = await repo.EmailExistsAsync("missing@example.com");

        Assert.False(result);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        var (repo, db) = Build(nameof(GetAllAsync_ReturnsAllUsers));
        db.Users.AddRange(NewUser("a@example.com"), NewUser("b@example.com"));
        await db.SaveChangesAsync();

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var (repo, _) = Build(nameof(GetAllAsync_EmptyDatabase_ReturnsEmptyList));

        var result = await repo.GetAllAsync();

        Assert.Empty(result);
    }

    // ── CountAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        var (repo, db) = Build(nameof(CountAsync_ReturnsCorrectCount));
        db.Users.AddRange(NewUser("a@example.com"), NewUser("b@example.com"), NewUser("c@example.com"));
        await db.SaveChangesAsync();

        var count = await repo.CountAsync();

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task CountAsync_EmptyDatabase_ReturnsZero()
    {
        var (repo, _) = Build(nameof(CountAsync_EmptyDatabase_ReturnsZero));

        var count = await repo.CountAsync();

        Assert.Equal(0, count);
    }
}
