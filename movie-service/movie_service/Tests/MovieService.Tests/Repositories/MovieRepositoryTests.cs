using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MovieService.Core.Data;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Tests.Repositories;

[Collection("postgres")]
public class MovieRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private MovieDbContext _db = null!;
    private MovieRepository _repo = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<MovieDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
        _db = new MovieDbContext(options);
        _repo = new MovieRepository(_db, NullLogger<MovieRepository>.Instance);
        await _db.Movies.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    private static Movie MakeMovie(string? title = null) => new()
    {
        Title = title ?? "Test Movie",
        Genre = "Action",
        Duration = 120,
        PosterUrl = "https://example.com/poster.jpg",
        IsActive = true
    };

    [Fact]
    public async Task AddAsync_PersistsAndReturnsMovie()
    {
        var movie = await _repo.AddAsync(MakeMovie("Inception"), CancellationToken.None);

        movie.Id.Should().BeGreaterThan(0);
        movie.Title.Should().Be("Inception");
        movie.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingMovie_ReturnsMovie()
    {
        var created = await _repo.AddAsync(MakeMovie(), CancellationToken.None);

        var found = await _repo.GetByIdAsync(created.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(99999, CancellationToken.None);

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMovies()
    {
        await _repo.AddAsync(MakeMovie("Movie A"), CancellationToken.None);
        await _repo.AddAsync(MakeMovie("Movie B"), CancellationToken.None);

        var all = await _repo.GetAllAsync(CancellationToken.None);

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var movie = await _repo.AddAsync(MakeMovie("Original"), CancellationToken.None);
        movie.Title = "Updated";

        var updated = await _repo.UpdateAsync(movie, CancellationToken.None);

        updated.Title.Should().Be("Updated");
        var refreshed = await _repo.GetByIdAsync(movie.Id, CancellationToken.None);
        refreshed!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_RemovesMovie()
    {
        var movie = await _repo.AddAsync(MakeMovie(), CancellationToken.None);

        await _repo.DeleteAsync(movie.Id, CancellationToken.None);

        var found = await _repo.GetByIdAsync(movie.Id, CancellationToken.None);
        found.Should().BeNull();
    }

    [Fact]
    public async Task Query_ReturnsIQueryable()
    {
        await _repo.AddAsync(MakeMovie("Action Movie"), CancellationToken.None);
        await _repo.AddAsync(MakeMovie("Drama Movie"), CancellationToken.None);

        var active = _repo.Query().Where(m => m.IsActive).ToList();

        active.Should().HaveCount(2);
    }
}
