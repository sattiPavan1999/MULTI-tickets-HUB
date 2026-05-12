using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using MovieService.Core.Data;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Mapping;
using MovieService.Core.Models;
using MovieService.Core.Repositories;
using MovieService.Core.Services;
using MovieService.Core.Validators;

namespace MovieService.Tests.Services;

public class BookingServiceTests
{
    private static IMapper BuildMapper()
        => new MapperConfiguration(c => c.AddProfile<MovieMappingProfile>()).CreateMapper();

    private static DbContextOptions<MovieDbContext> BuildOptions(string dbName) =>
        new DbContextOptionsBuilder<MovieDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static (IBookingService bookingSvc, IShowtimeService showtimeSvc, MovieDbContext db) BuildFullService(string dbName)
    {
        var db = new MovieDbContext(BuildOptions(dbName));
        var movieRepo = new MovieRepository(db, NullLogger<MovieRepository>.Instance);
        var showtimeRepo = new ShowtimeRepository(db, NullLogger<ShowtimeRepository>.Instance);
        var bookingRepo = new BookingRepository(db);
        var mapper = BuildMapper();

        var showtimeSvc = new ShowtimeService(
            showtimeRepo,
            movieRepo,
            bookingRepo,
            new CreateShowtimeInputValidator(),
            mapper,
            NullLogger<ShowtimeService>.Instance);

        var bookingSvc = new BookingService(
            new CreateBookingInputValidator(),
            mapper,
            db,
            NullLogger<BookingService>.Instance);

        return (bookingSvc, showtimeSvc, db);
    }

    private static async Task<(Movie movie, Showtime showtime)> SeedAsync(MovieDbContext db, IShowtimeService showtimeSvc)
    {
        var movie = new Movie { Title = "Inception", Genre = "Sci-Fi", Duration = 148, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        var showtimeResponse = await showtimeSvc.CreateShowtimeAsync(new CreateShowtimeInput
        {
            MovieId = movie.Id,
            ShowDate = "2026-12-25",
            ShowTime = "14:30",
            ScreenNumber = "Screen 1",
            TotalSeats = 50
        });

        var showtime = db.Showtimes.Find(showtimeResponse.Id)!;
        return (movie, showtime);
    }

    [Fact]
    public async Task CreateBooking_ValidInput_ReturnsConfirmedBooking()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(CreateBooking_ValidInput_ReturnsConfirmedBooking));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);

        var result = await bookingSvc.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = showtime.Id,
            UserId = 42,
            SeatNumbers = [1, 2, 3]
        });

        result.Status.Should().Be("Confirmed");
        result.NumberOfSeats.Should().Be(3);
        result.UserId.Should().Be(42);
        result.SeatNumbers.Should().Be("1,2,3");
    }

    [Fact]
    public async Task CreateBooking_ShowtimeNotFound_ThrowsNotFoundException()
    {
        var (bookingSvc, _, _) = BuildFullService(nameof(CreateBooking_ShowtimeNotFound_ThrowsNotFoundException));

        await bookingSvc.Invoking(s => s.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = 9999,
            UserId = 1,
            SeatNumbers = [1]
        })).Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateBooking_SeatAlreadyBooked_ThrowsConflictException()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(CreateBooking_SeatAlreadyBooked_ThrowsConflictException));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);

        await bookingSvc.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [5, 6]
        });

        await bookingSvc.Invoking(s => s.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = showtime.Id, UserId = 2, SeatNumbers = [5, 7]
        })).Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateBooking_ExceedsAvailableSeats_ThrowsConflictException()
    {
        var db = new MovieDbContext(BuildOptions(nameof(CreateBooking_ExceedsAvailableSeats_ThrowsConflictException)));
        var movie = new Movie { Title = "T", Genre = "G", Duration = 100, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie);
        var showtime = new Showtime
        {
            Movie = movie,
            ShowDate = new DateOnly(2027, 1, 1),
            ShowTime = new TimeOnly(10, 0),
            ScreenNumber = "S1",
            TotalSeats = 2,
            AvailableSeats = 2
        };
        db.Showtimes.Add(showtime);
        await db.SaveChangesAsync();

        var bookingSvc = new BookingService(new CreateBookingInputValidator(), BuildMapper(), db, NullLogger<BookingService>.Instance);

        await bookingSvc.Invoking(s => s.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1, 2, 3]
        })).Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateBooking_DecrementsAvailableSeatsOnShowtime()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(CreateBooking_DecrementsAvailableSeatsOnShowtime));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);

        await bookingSvc.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1, 2]
        });

        db.ChangeTracker.Clear();
        var refreshed = db.Showtimes.Find(showtime.Id)!;
        refreshed.AvailableSeats.Should().Be(48);
    }

    [Fact]
    public async Task CreateBooking_InvalidInput_ThrowsValidationException()
    {
        var (bookingSvc, _, _) = BuildFullService(nameof(CreateBooking_InvalidInput_ThrowsValidationException));

        await bookingSvc.Invoking(s => s.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = 0,
            UserId = 0,
            SeatNumbers = []
        })).Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task CreateBooking_PastShowtime_ThrowsConflictException()
    {
        var db = new MovieDbContext(BuildOptions(nameof(CreateBooking_PastShowtime_ThrowsConflictException)));
        var movie = new Movie { Title = "Old Film", Genre = "Drama", Duration = 120, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie);
        var pastShowtime = new Showtime
        {
            Movie = movie,
            ShowDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            ShowTime = new TimeOnly(10, 0),
            ScreenNumber = "S1",
            TotalSeats = 50,
            AvailableSeats = 50
        };
        db.Showtimes.Add(pastShowtime);
        await db.SaveChangesAsync();

        var bookingSvc = new BookingService(new CreateBookingInputValidator(), BuildMapper(), db, NullLogger<BookingService>.Instance);

        await bookingSvc.Invoking(s => s.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = pastShowtime.Id,
            UserId = 1,
            SeatNumbers = [1]
        })).Should().ThrowAsync<ConflictException>()
            .WithMessage("*already started*");
    }
}
