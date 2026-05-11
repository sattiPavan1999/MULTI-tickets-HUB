using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MovieService.Core.Data;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Tests.Repositories;

[Collection("postgres")]
public class ShowtimeRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private MovieDbContext _db = null!;
    private ShowtimeRepository _repo = null!;
    private Movie _movie = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<MovieDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
        _db = new MovieDbContext(options);
        _repo = new ShowtimeRepository(_db, NullLogger<ShowtimeRepository>.Instance);

        await _db.Bookings.ExecuteDeleteAsync();
        await _db.Showtimes.ExecuteDeleteAsync();
        await _db.Movies.ExecuteDeleteAsync();

        _movie = new Movie { Title = "Test Movie", Genre = "Action", Duration = 120, PosterUrl = "https://example.com/p.jpg" };
        _db.Movies.Add(_movie);
        await _db.SaveChangesAsync();
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    private Showtime MakeShowtime(DateOnly? date = null, TimeOnly? time = null, string screen = "Screen 1") => new()
    {
        MovieId = _movie.Id,
        ShowDate = date ?? new DateOnly(2026, 12, 25),
        ShowTime = time ?? new TimeOnly(14, 30),
        ScreenNumber = screen,
        TotalSeats = 50,
        AvailableSeats = 50
    };

    [Fact]
    public async Task AddAsync_PersistsShowtime()
    {
        var showtime = await _repo.AddAsync(MakeShowtime());

        showtime.Id.Should().BeGreaterThan(0);
        showtime.TotalSeats.Should().Be(50);
        showtime.AvailableSeats.Should().Be(50);
    }

    [Fact]
    public async Task GetByMovieIdAsync_ReturnsMatchingShowtimes()
    {
        await _repo.AddAsync(MakeShowtime(new DateOnly(2026, 12, 25)));
        await _repo.AddAsync(MakeShowtime(new DateOnly(2026, 12, 26)));

        var result = await _repo.GetByMovieIdAsync(_movie.Id);

        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(s => s.ShowDate);
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_ExistingSlot_Returns()
    {
        var showtime = MakeShowtime();
        await _repo.AddAsync(showtime);

        var result = await _repo.GetByCompositeKeyAsync(
            _movie.Id, showtime.ShowDate, showtime.ShowTime, showtime.ScreenNumber);

        result.Should().NotBeNull();
        result!.Id.Should().Be(showtime.Id);
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_NonExistingSlot_ReturnsNull()
    {
        var result = await _repo.GetByCompositeKeyAsync(
            _movie.Id, new DateOnly(2027, 1, 1), new TimeOnly(10, 0), "Screen 99");

        result.Should().BeNull();
    }
}
