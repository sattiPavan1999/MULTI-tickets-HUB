using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieService.Core.Data;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Mapping;
using MovieService.Core.Models;
using MovieService.Core.Repositories;
using MovieService.Core.Services;
using MovieService.Core.Validators;

namespace MovieService.Tests.Services;

public class ShowtimeServiceTests
{
    private static IMapper BuildMapper()
        => new MapperConfiguration(c => c.AddProfile<MovieMappingProfile>()).CreateMapper();

    private static (IShowtimeService svc, MovieDbContext db) BuildFullService(string dbName)
    {
        var options = new DbContextOptionsBuilder<MovieDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new MovieDbContext(options);
        var movieRepo = new MovieRepository(db, NullLogger<MovieRepository>.Instance);
        var showtimeRepo = new ShowtimeRepository(db, NullLogger<ShowtimeRepository>.Instance);
        var bookingRepo = new BookingRepository(db);
        var svc = new ShowtimeService(
            showtimeRepo,
            movieRepo,
            bookingRepo,
            new CreateShowtimeInputValidator(),
            BuildMapper(),
            NullLogger<ShowtimeService>.Instance);
        return (svc, db);
    }

    private static async Task<Movie> SeedMovieAsync(MovieDbContext db)
    {
        var movie = new Movie { Title = "Inception", Genre = "Sci-Fi", Duration = 148, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
        return movie;
    }

    private static CreateShowtimeInput ValidInput(int movieId) => new()
    {
        MovieId = movieId,
        ShowDate = "2026-12-25",
        ShowTime = "14:30",
        ScreenNumber = "Screen 1",
        TotalSeats = 50
    };

    [Fact]
    public async Task CreateShowtime_ValidInput_ReturnsResponse()
    {
        var (svc, db) = BuildFullService(nameof(CreateShowtime_ValidInput_ReturnsResponse));
        var movie = await SeedMovieAsync(db);

        var result = await svc.CreateShowtimeAsync(ValidInput(movie.Id));

        result.MovieId.Should().Be(movie.Id);
        result.TotalSeats.Should().Be(50);
        result.AvailableSeats.Should().Be(50);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateShowtime_MovieNotFound_ThrowsNotFoundException()
    {
        var (svc, _) = BuildFullService(nameof(CreateShowtime_MovieNotFound_ThrowsNotFoundException));

        await svc.Invoking(s => s.CreateShowtimeAsync(ValidInput(9999)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateShowtime_DuplicateSlot_ThrowsConflictException()
    {
        var (svc, db) = BuildFullService(nameof(CreateShowtime_DuplicateSlot_ThrowsConflictException));
        var movie = await SeedMovieAsync(db);
        var input = ValidInput(movie.Id);
        await svc.CreateShowtimeAsync(input);

        await svc.Invoking(s => s.CreateShowtimeAsync(ValidInput(movie.Id)))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateShowtime_DifferentMovieSameScreenWithin4Hours_ThrowsConflictException()
    {
        var (svc, db) = BuildFullService(nameof(CreateShowtime_DifferentMovieSameScreenWithin4Hours_ThrowsConflictException));
        var movie1 = await SeedMovieAsync(db);
        var movie2 = new Movie { Title = "Interstellar", Genre = "Sci-Fi", Duration = 169, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie2);
        await db.SaveChangesAsync();

        // Movie1: Screen 1, 14:30
        await svc.CreateShowtimeAsync(new CreateShowtimeInput
        {
            MovieId = movie1.Id, ShowDate = "2026-12-25", ShowTime = "14:30",
            ScreenNumber = "Screen 1", TotalSeats = 50
        });

        // Movie2: same screen, 16:00 — only 1.5h gap, should be rejected
        await svc.Invoking(s => s.CreateShowtimeAsync(new CreateShowtimeInput
        {
            MovieId = movie2.Id, ShowDate = "2026-12-25", ShowTime = "16:00",
            ScreenNumber = "Screen 1", TotalSeats = 50
        })).Should().ThrowAsync<ConflictException>()
            .WithMessage("*4-hour gap*");
    }

    [Fact]
    public async Task CreateShowtime_DifferentMovieSameScreenExactly4HoursGap_Succeeds()
    {
        var (svc, db) = BuildFullService(nameof(CreateShowtime_DifferentMovieSameScreenExactly4HoursGap_Succeeds));
        var movie1 = await SeedMovieAsync(db);
        var movie2 = new Movie { Title = "Interstellar", Genre = "Sci-Fi", Duration = 169, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie2);
        await db.SaveChangesAsync();

        await svc.CreateShowtimeAsync(new CreateShowtimeInput
        {
            MovieId = movie1.Id, ShowDate = "2026-12-25", ShowTime = "14:30",
            ScreenNumber = "Screen 1", TotalSeats = 50
        });

        // Exactly 4 hours later — should be allowed (gap is not strictly less than 4h)
        var result = await svc.CreateShowtimeAsync(new CreateShowtimeInput
        {
            MovieId = movie2.Id, ShowDate = "2026-12-25", ShowTime = "18:30",
            ScreenNumber = "Screen 1", TotalSeats = 50
        });

        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetShowtimesByMovie_ReturnsOrderedList()
    {
        var (svc, db) = BuildFullService(nameof(GetShowtimesByMovie_ReturnsOrderedList));
        var movie = await SeedMovieAsync(db);

        var input1 = ValidInput(movie.Id);
        input1.ShowDate = "2026-12-26";
        var input2 = ValidInput(movie.Id);
        input2.ShowDate = "2026-12-25";

        await svc.CreateShowtimeAsync(input1);
        await svc.CreateShowtimeAsync(input2);

        var result = await svc.GetShowtimesByMovieAsync(movie.Id);

        result.Should().HaveCount(2);
        result[0].ShowDate.Should().Be(new DateOnly(2026, 12, 25));
    }

    [Fact]
    public async Task GetSeatStatus_NoBookings_ReturnsEmptyBookedSeats()
    {
        var (svc, db) = BuildFullService(nameof(GetSeatStatus_NoBookings_ReturnsEmptyBookedSeats));
        var movie = await SeedMovieAsync(db);
        var showtime = await svc.CreateShowtimeAsync(ValidInput(movie.Id));

        var result = await svc.GetSeatStatusAsync(showtime.Id);

        result.TotalSeats.Should().Be(50);
        result.BookedSeats.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSeatStatus_WithBookings_ReturnsCorrectBookedSeats()
    {
        var bookingRepo = new Mock<IBookingRepository>();
        bookingRepo.Setup(r => r.GetByShowtimeAsync(1))
            .ReturnsAsync([
                new MovieBooking { SeatNumbers = "1,3,5", Status = "Pending" },
                new MovieBooking { SeatNumbers = "7", Status = "Confirmed" }
            ]);

        var options = new DbContextOptionsBuilder<MovieDbContext>()
            .UseInMemoryDatabase(nameof(GetSeatStatus_WithBookings_ReturnsCorrectBookedSeats))
            .Options;
        var db = new MovieDbContext(options);
        var movie = new Movie { Title = "Test", Genre = "G", Duration = 100, PosterUrl = "https://example.com/p.jpg" };
        db.Movies.Add(movie);
        var showtime = new Showtime { MovieId = movie.Id, ShowDate = new DateOnly(2026, 1, 1), ShowTime = new TimeOnly(10, 0), ScreenNumber = "S1", TotalSeats = 50, AvailableSeats = 46 };
        db.Movies.Add(movie);
        db.Showtimes.Add(showtime);
        await db.SaveChangesAsync();

        var movieRepo = new MovieRepository(db, NullLogger<MovieRepository>.Instance);
        var showtimeRepo = new ShowtimeRepository(db, NullLogger<ShowtimeRepository>.Instance);
        var svc = new ShowtimeService(
            showtimeRepo,
            movieRepo,
            bookingRepo.Object,
            new CreateShowtimeInputValidator(),
            BuildMapper(),
            NullLogger<ShowtimeService>.Instance);

        var result = await svc.GetSeatStatusAsync(showtime.Id);

        result.BookedSeats.Should().BeEquivalentTo([1, 3, 5, 7]);
    }

    [Fact]
    public async Task DeleteShowtime_UnknownId_ThrowsNotFoundException()
    {
        var (svc, _) = BuildFullService(nameof(DeleteShowtime_UnknownId_ThrowsNotFoundException));

        await svc.Invoking(s => s.DeleteShowtimeAsync(9999))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetShowtimesByMovie_ExcludesPastShowtimes()
    {
        var (svc, db) = BuildFullService(nameof(GetShowtimesByMovie_ExcludesPastShowtimes));
        var movie = await SeedMovieAsync(db);

        // Directly insert a past showtime (bypassing service to avoid future-only constraint)
        db.Showtimes.Add(new Showtime
        {
            MovieId = movie.Id,
            ShowDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            ShowTime = new TimeOnly(10, 0),
            ScreenNumber = "Screen 1",
            TotalSeats = 50,
            AvailableSeats = 50
        });
        await db.SaveChangesAsync();

        // Create one future showtime via the service
        await svc.CreateShowtimeAsync(ValidInput(movie.Id));

        var result = await svc.GetShowtimesByMovieAsync(movie.Id);

        result.Should().HaveCount(1);
        result[0].ShowDate.Should().Be(new DateOnly(2026, 12, 25));
    }
}
