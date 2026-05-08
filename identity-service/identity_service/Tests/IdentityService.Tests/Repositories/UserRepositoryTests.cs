using Bogus;
using IdentityService.Core.Data;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IdentityService.Tests.Repositories;

[Collection("postgres")]
public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private IdentityDbContext _context = null!;
    private UserRepository _repo = null!;

    private static readonly Faker<User> UserFaker = new Faker<User>()
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.PasswordHash, _ => "hashed_password")
        .RuleFor(u => u.FullName, f => f.Name.FullName())
        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("+1##########"))
        .RuleFor(u => u.Role, _ => "User");

    public UserRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        _context = new IdentityDbContext(options);

        // Clean data only — migrations already applied by the fixture
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        _repo = new UserRepository(_context, NullLogger<UserRepository>.Instance);
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsUser_AndReturnsWithId()
    {
        var user = UserFaker.Generate();

        var created = await _repo.AddAsync(user);

        created.Id.Should().BeGreaterThan(0);
        (await _context.Users.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_SetsCreatedAt()
    {
        var before = DateTime.UtcNow;
        var user = UserFaker.Generate();

        var created = await _repo.AddAsync(user);

        created.CreatedAt.Should().BeOnOrAfter(before);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var user = UserFaker.Generate();
        await _repo.AddAsync(user);

        var result = await _repo.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ── GetByEmailAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        var user = UserFaker.Generate();
        await _repo.AddAsync(user);

        var result = await _repo.GetByEmailAsync(user.Email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        var result = await _repo.GetByEmailAsync("nobody@example.com");

        result.Should().BeNull();
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var user = UserFaker.Generate();
        await _repo.AddAsync(user);

        user.FullName = "Updated Name";
        await _repo.UpdateAsync(user);

        var refreshed = await _context.Users.FindAsync(user.Id);
        refreshed!.FullName.Should().Be("Updated Name");
    }

    // ── EmailExistsAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        var user = UserFaker.Generate();
        await _repo.AddAsync(user);

        var result = await _repo.EmailExistsAsync(user.Email);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
    {
        var result = await _repo.EmailExistsAsync("missing@example.com");

        result.Should().BeFalse();
    }

    // ── GetAllAsync / CountAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        var users = UserFaker.Generate(3);
        foreach (var u in users)
            await _repo.AddAsync(u);

        var result = await _repo.GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        var users = UserFaker.Generate(2);
        foreach (var u in users)
            await _repo.AddAsync(u);

        var count = await _repo.CountAsync();

        count.Should().Be(2);
    }

    // ── Query() ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Query_CanFilterByEmail()
    {
        var users = UserFaker.Generate(3);
        foreach (var u in users)
            await _repo.AddAsync(u);

        var target = users[0].Email;
        var result = _repo.Query().Where(u => u.Email == target).ToList();

        result.Should().ContainSingle(u => u.Email == target);
    }
}
