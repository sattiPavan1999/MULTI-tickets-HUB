using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MovieService.Core.Data;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Tests.Repositories;

[Collection("postgres")]
public class BookingRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private MovieDbContext _db = null!;
    private BookingRepository _repo = null!;
    private Showtime _showtime = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<MovieDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
        _db = new MovieDbContext(options);
        _repo = new BookingRepository(_db);

        await _db.Bookings.ExecuteDeleteAsync();
        await _db.Showtimes.ExecuteDeleteAsync();
        await _db.Movies.ExecuteDeleteAsync();

        var movie = new Movie { Title = "Test", Genre = "Action", Duration = 100, PosterUrl = "https://example.com/p.jpg" };
        _db.Movies.Add(movie);
        await _db.SaveChangesAsync();

        _showtime = new Showtime
        {
            MovieId = movie.Id,
            ShowDate = new DateOnly(2026, 12, 25),
            ShowTime = new TimeOnly(14, 30),
            ScreenNumber = "Screen 1",
            TotalSeats = 50,
            AvailableSeats = 50
        };
        _db.Showtimes.Add(_showtime);
        await _db.SaveChangesAsync();
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    private MovieBooking MakeBooking(string seats = "1,2", string status = "Pending") => new()
    {
        ShowtimeId = _showtime.Id,
        UserId = 1,
        SeatNumbers = seats,
        NumberOfSeats = seats.Split(',').Length,
        Status = status,
        BookedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
    };

    [Fact]
    public async Task AddAsync_PersistsBooking()
    {
        var booking = await _repo.AddAsync(MakeBooking());

        booking.Id.Should().BeGreaterThan(0);
        booking.Status.Should().Be("Pending");
        booking.SeatNumbers.Should().Be("1,2");
    }

    [Fact]
    public async Task GetByShowtimeAsync_ReturnsPendingAndConfirmed()
    {
        await _repo.AddAsync(MakeBooking("1,2", "Pending"));
        await _repo.AddAsync(MakeBooking("3,4", "Confirmed"));

        var result = await _repo.GetByShowtimeAsync(_showtime.Id);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByShowtimeAsync_ExcludesCancelledBookings()
    {
        await _repo.AddAsync(MakeBooking("1,2", "Pending"));
        await _repo.AddAsync(MakeBooking("3,4", "Cancelled"));

        var result = await _repo.GetByShowtimeAsync(_showtime.Id);

        result.Should().HaveCount(1);
        result[0].SeatNumbers.Should().Be("1,2");
    }
}
