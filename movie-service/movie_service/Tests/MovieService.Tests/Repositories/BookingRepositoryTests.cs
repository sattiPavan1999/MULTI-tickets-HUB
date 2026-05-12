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

    private static DateTime Unspecified(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

    private MovieBooking MakeBooking(string seats = "1,2", string status = "Pending") => new()
    {
        ShowtimeId = _showtime.Id,
        UserId = 1,
        SeatNumbers = seats,
        NumberOfSeats = seats.Split(',').Length,
        Status = status,
        BookedAt = Unspecified(DateTime.UtcNow)
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

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserBookingsOrderedDesc()
    {
        var b1 = MakeBooking("1", "Confirmed"); b1.UserId = 10; b1.BookedAt = Unspecified(DateTime.UtcNow.AddMinutes(-5));
        var b2 = MakeBooking("2", "Confirmed"); b2.UserId = 10; b2.BookedAt = Unspecified(DateTime.UtcNow);
        var b3 = MakeBooking("3", "Confirmed"); b3.UserId = 99;
        await _repo.AddAsync(b1);
        await _repo.AddAsync(b2);
        await _repo.AddAsync(b3);

        var result = await _repo.GetByUserIdAsync(10);

        result.Should().HaveCount(2);
        result[0].BookedAt.Should().BeAfter(result[1].BookedAt);
    }

    [Fact]
    public async Task GetByUserIdAsync_LoadsShowtimeAndMovieNavigation()
    {
        var booking = MakeBooking("5", "Confirmed");
        booking.UserId = 20;
        await _repo.AddAsync(booking);

        var result = await _repo.GetByUserIdAsync(20);

        result.Should().HaveCount(1);
        result[0].Showtime.Should().NotBeNull();
        result[0].Showtime.Movie.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ReturnsNullWhenNotFound()
    {
        var result = await _repo.GetByIdWithDetailsAsync(99999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_LoadsShowtimeNavigation()
    {
        var booking = await _repo.AddAsync(MakeBooking("7", "Confirmed"));

        var result = await _repo.GetByIdWithDetailsAsync(booking.Id);

        result.Should().NotBeNull();
        result!.Showtime.Should().NotBeNull();
        result.Showtime.Movie.Should().NotBeNull();
    }
}
