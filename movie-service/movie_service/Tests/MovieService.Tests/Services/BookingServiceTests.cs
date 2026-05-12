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
            bookingRepo,
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

        var bookingSvc = new BookingService(new CreateBookingInputValidator(), BuildMapper(), db, new BookingRepository(db), NullLogger<BookingService>.Instance);

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

        var bookingSvc = new BookingService(new CreateBookingInputValidator(), BuildMapper(), db, new BookingRepository(db), NullLogger<BookingService>.Instance);

        await bookingSvc.Invoking(s => s.CreateBookingAsync(new CreateBookingInput
        {
            ShowtimeId = pastShowtime.Id,
            UserId = 1,
            SeatNumbers = [1]
        })).Should().ThrowAsync<ConflictException>()
            .WithMessage("*already started*");
    }

    [Fact]
    public async Task GetMyBookings_ReturnsAllUserBookingsOrderedDesc()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(GetMyBookings_ReturnsAllUserBookingsOrderedDesc));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);

        await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1] });
        await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [2] });
        await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 2, SeatNumbers = [3] });

        var results = await bookingSvc.GetMyBookingsAsync(1);

        results.Should().HaveCount(2);
        results.Should().BeInDescendingOrder(b => b.BookedAt);
    }

    [Fact]
    public async Task GetMyBookings_ReturnsEmpty_WhenNoBookings()
    {
        var (bookingSvc, _, _) = BuildFullService(nameof(GetMyBookings_ReturnsEmpty_WhenNoBookings));

        var results = await bookingSvc.GetMyBookingsAsync(999);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBookingById_ReturnsEnrichedBooking()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(GetBookingById_ReturnsEnrichedBooking));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);
        var created = await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1] });

        var result = await bookingSvc.GetBookingByIdAsync(created.Id, 1);

        result.Id.Should().Be(created.Id);
        result.MovieTitle.Should().Be("Inception");
    }

    [Fact]
    public async Task GetBookingById_ThrowsNotFound_WhenMissing()
    {
        var (bookingSvc, _, _) = BuildFullService(nameof(GetBookingById_ThrowsNotFound_WhenMissing));

        await bookingSvc.Invoking(s => s.GetBookingByIdAsync(9999, 1))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBookingById_ThrowsUnauthorized_WhenWrongUser()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(GetBookingById_ThrowsUnauthorized_WhenWrongUser));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);
        var created = await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1] });

        await bookingSvc.Invoking(s => s.GetBookingByIdAsync(created.Id, 2))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authorized to view*");
    }

    [Fact]
    public async Task CancelBooking_SetsStatusCancelled_RestoresSeats()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(CancelBooking_SetsStatusCancelled_RestoresSeats));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);
        var created = await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1, 2] });

        var result = await bookingSvc.CancelBookingAsync(created.Id, 1);

        result.Success.Should().BeTrue();
        db.ChangeTracker.Clear();
        db.Bookings.Find(created.Id)!.Status.Should().Be("Cancelled");
        db.Showtimes.Find(showtime.Id)!.AvailableSeats.Should().Be(showtime.TotalSeats);
    }

    [Fact]
    public async Task CancelBooking_ThrowsConflict_WhenAlreadyCancelled()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(CancelBooking_ThrowsConflict_WhenAlreadyCancelled));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);
        var created = await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1] });
        await bookingSvc.CancelBookingAsync(created.Id, 1);

        await bookingSvc.Invoking(s => s.CancelBookingAsync(created.Id, 1))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public async Task CancelBooking_ThrowsConflict_WhenWithin2HoursOfShow()
    {
        var db = new MovieDbContext(BuildOptions(nameof(CancelBooking_ThrowsConflict_WhenWithin2HoursOfShow)));
        var movie = new Movie { Title = "Upcoming", Genre = "Action", Duration = 120, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie);
        var nearShowtime = new Showtime
        {
            Movie = movie,
            ShowDate = DateOnly.FromDateTime(DateTime.Now),
            ShowTime = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(90)),
            ScreenNumber = "S1",
            TotalSeats = 50,
            AvailableSeats = 50
        };
        db.Showtimes.Add(nearShowtime);
        await db.SaveChangesAsync();

        var bookingSvc = new BookingService(new CreateBookingInputValidator(), BuildMapper(), db, new BookingRepository(db), NullLogger<BookingService>.Instance);
        var booking = new MovieBooking { ShowtimeId = nearShowtime.Id, UserId = 1, SeatNumbers = "1", NumberOfSeats = 1, Status = "Confirmed", BookedAt = DateTime.UtcNow };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        await bookingSvc.Invoking(s => s.CancelBookingAsync(booking.Id, 1))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*2 hours*");
    }

    [Fact]
    public async Task CancelBooking_ThrowsUnauthorized_WhenWrongUser()
    {
        var (bookingSvc, showtimeSvc, db) = BuildFullService(nameof(CancelBooking_ThrowsUnauthorized_WhenWrongUser));
        var (_, showtime) = await SeedAsync(db, showtimeSvc);
        var created = await bookingSvc.CreateBookingAsync(new CreateBookingInput { ShowtimeId = showtime.Id, UserId = 1, SeatNumbers = [1] });

        await bookingSvc.Invoking(s => s.CancelBookingAsync(created.Id, 2))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authorized to cancel*");
    }

    [Fact]
    public async Task CancelBooking_ThrowsNotFound_WhenMissing()
    {
        var (bookingSvc, _, _) = BuildFullService(nameof(CancelBooking_ThrowsNotFound_WhenMissing));

        await bookingSvc.Invoking(s => s.CancelBookingAsync(9999, 1))
            .Should().ThrowAsync<NotFoundException>();
    }
}
