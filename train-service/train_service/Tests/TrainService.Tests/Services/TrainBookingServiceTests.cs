using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using TrainService.Core.Data;
using TrainService.Core.DTOs;
using TrainService.Core.Exceptions;
using TrainService.Core.Mapping;
using TrainService.Core.Models;
using TrainService.Core.Repositories;
using TrainService.Core.Services;
using TrainService.Core.Validators;

namespace TrainService.Tests.Services;

public class TrainBookingServiceTests
{
    private static IMapper BuildMapper()
        => new MapperConfiguration(c => c.AddProfile<TrainMappingProfile>()).CreateMapper();

    private static (ITrainBookingService svc, TrainDbContext db, Train train) BuildFullService(string dbName, int availableSeats = 10)
    {
        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new TrainDbContext(options);
        var trainRepo = new TrainRepository(db, NullLogger<TrainRepository>.Instance);
        var seatRepo = new SeatAvailabilityRepository(db);
        var bookingRepo = new TrainBookingRepository(db);

        var train = new Train
        {
            TrainName = "Test Express",
            TrainNumber = $"T{Guid.NewGuid():N}"[..8],
            Source = "City A",
            Destination = "City B",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(8),
            Price = 500m
        };
        db.Trains.Add(train);
        db.SaveChanges();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.SeatAvailabilities.Add(new SeatAvailability { TrainId = train.Id, Date = today, AvailableSeats = availableSeats });
        db.SaveChanges();

        var svc = new TrainBookingService(
            bookingRepo, seatRepo, trainRepo,
            new CreateTrainBookingInputValidator(),
            BuildMapper(), db,
            NullLogger<TrainBookingService>.Instance);

        return (svc, db, train);
    }

    private static void AddStops(TrainDbContext db, Train train)
    {
        db.TrainStops.AddRange(
            new TrainStop { TrainId = train.Id, StopNumber = 1, StationName = "City A" },
            new TrainStop { TrainId = train.Id, StopNumber = 2, StationName = "City M" },
            new TrainStop { TrainId = train.Id, StopNumber = 3, StationName = "City B" }
        );
        db.SaveChanges();
    }

    private static CreateTrainBookingInput ValidInput(int trainId) => new()
    {
        TrainId = trainId,
        UserId = 1,
        TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
        PassengerName = "Alice",
        PassengerAge = 28,
        NumberOfSeats = 2
    };

    [Fact]
    public async Task CreateBooking_ValidInput_ReturnsConfirmed()
    {
        var (svc, _, train) = BuildFullService(nameof(CreateBooking_ValidInput_ReturnsConfirmed));

        var result = await svc.CreateBookingAsync(ValidInput(train.Id));

        result.Status.Should().Be("Confirmed");
        result.WaitlistPosition.Should().BeNull();
        result.PNR.Should().StartWith("PNR");
        result.PNR.Length.Should().Be(11);
    }

    [Fact]
    public async Task CreateBooking_NoSeatAvailability_ThrowsNotFoundException()
    {
        var (svc, _, _) = BuildFullService(nameof(CreateBooking_NoSeatAvailability_ThrowsNotFoundException));

        var input = new CreateTrainBookingInput
        {
            TrainId = 99999, UserId = 1,
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            PassengerName = "Bob", PassengerAge = 30, NumberOfSeats = 1
        };

        await svc.Invoking(s => s.CreateBookingAsync(input))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateBooking_AvailableSeatsZero_ReturnsWaitlisted()
    {
        var (svc, _, train) = BuildFullService(nameof(CreateBooking_AvailableSeatsZero_ReturnsWaitlisted), availableSeats: 0);

        var result = await svc.CreateBookingAsync(ValidInput(train.Id));

        result.Status.Should().Be("Waitlisted");
        result.WaitlistPosition.Should().Be(1);
    }

    [Fact]
    public async Task CreateBooking_WaitlistPosition_IsSequential()
    {
        var (svc, _, train) = BuildFullService(nameof(CreateBooking_WaitlistPosition_IsSequential), availableSeats: 0);

        var r1 = await svc.CreateBookingAsync(ValidInput(train.Id));
        var r2 = await svc.CreateBookingAsync(ValidInput(train.Id));
        var r3 = await svc.CreateBookingAsync(ValidInput(train.Id));

        r1.WaitlistPosition.Should().Be(1);
        r2.WaitlistPosition.Should().Be(2);
        r3.WaitlistPosition.Should().Be(3);
    }

    [Fact]
    public async Task CreateBooking_InsufficientPartialSeats_ThrowsConflictException()
    {
        var (svc, _, train) = BuildFullService(nameof(CreateBooking_InsufficientPartialSeats_ThrowsConflictException), availableSeats: 1);

        var input = ValidInput(train.Id);
        input.NumberOfSeats = 3;

        await svc.Invoking(s => s.CreateBookingAsync(input))
            .Should().ThrowAsync<ConflictException>().WithMessage("*Only 1 seat(s) available*");
    }

    [Fact]
    public async Task CreateBooking_DecrementsAvailableSeats()
    {
        var (svc, db, train) = BuildFullService(nameof(CreateBooking_DecrementsAvailableSeats), availableSeats: 10);
        var input = ValidInput(train.Id);
        input.NumberOfSeats = 3;

        await svc.CreateBookingAsync(input);

        db.ChangeTracker.Clear();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var seat = await db.SeatAvailabilities.FirstAsync(s => s.TrainId == train.Id && s.Date == today);
        seat.AvailableSeats.Should().Be(7);
    }

    [Fact]
    public async Task CreateBooking_DepartsWithinOneHour_ThrowsConflictException()
    {
        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseInMemoryDatabase(nameof(CreateBooking_DepartsWithinOneHour_ThrowsConflictException))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new TrainDbContext(options);
        var trainRepo = new TrainRepository(db, NullLogger<TrainRepository>.Instance);
        var seatRepo = new SeatAvailabilityRepository(db);
        var bookingRepo = new TrainBookingRepository(db);

        var soonTrain = new Train
        {
            TrainName = "Soon Express",
            TrainNumber = $"S{Guid.NewGuid():N}"[..6],
            Source = "A",
            Destination = "B",
            DepartureTime = DateTime.UtcNow.AddMinutes(30),
            ArrivalTime = DateTime.UtcNow.AddHours(4),
            Price = 500m
        };
        db.Trains.Add(soonTrain);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.SeatAvailabilities.Add(new SeatAvailability { TrainId = soonTrain.Id, Date = today, AvailableSeats = 10 });
        db.SaveChanges();

        var svc = new TrainBookingService(bookingRepo, seatRepo, trainRepo,
            new CreateTrainBookingInputValidator(), BuildMapper(), db,
            NullLogger<TrainBookingService>.Instance);

        var input = new CreateTrainBookingInput
        {
            TrainId = soonTrain.Id, UserId = 1,
            TravelDate = today.ToString("yyyy-MM-dd"),
            PassengerName = "Bob", PassengerAge = 30, NumberOfSeats = 1
        };

        await svc.Invoking(s => s.CreateBookingAsync(input))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*Booking is closed*");
    }

    [Fact]
    public async Task CreateBooking_PNRIsUnique()
    {
        var (svc, _, train) = BuildFullService(nameof(CreateBooking_PNRIsUnique), availableSeats: 100);

        var r1 = await svc.CreateBookingAsync(ValidInput(train.Id));
        var r2 = await svc.CreateBookingAsync(ValidInput(train.Id));

        r1.PNR.Should().NotBe(r2.PNR);
    }

    [Fact]
    public async Task CreateBooking_ValidBoardingAlighting_StoresStations()
    {
        var (svc, db, train) = BuildFullService(nameof(CreateBooking_ValidBoardingAlighting_StoresStations));
        AddStops(db, train);

        var input = ValidInput(train.Id);
        input.BoardingStation = "City A";
        input.AlightingStation = "City M";

        var result = await svc.CreateBookingAsync(input);

        result.Status.Should().Be("Confirmed");
        result.BoardingStation.Should().Be("City A");
        result.AlightingStation.Should().Be("City M");
    }

    [Fact]
    public async Task CreateBooking_InvalidBoardingStation_ThrowsNotFoundException()
    {
        var (svc, db, train) = BuildFullService(nameof(CreateBooking_InvalidBoardingStation_ThrowsNotFoundException));
        AddStops(db, train);

        var input = ValidInput(train.Id);
        input.BoardingStation = "Nonexistent Station";

        await svc.Invoking(s => s.CreateBookingAsync(input))
            .Should().ThrowAsync<NotFoundException>().WithMessage("*Boarding station*");
    }

    [Fact]
    public async Task CreateBooking_InvalidAlightingStation_ThrowsNotFoundException()
    {
        var (svc, db, train) = BuildFullService(nameof(CreateBooking_InvalidAlightingStation_ThrowsNotFoundException));
        AddStops(db, train);

        var input = ValidInput(train.Id);
        input.AlightingStation = "Ghost Station";

        await svc.Invoking(s => s.CreateBookingAsync(input))
            .Should().ThrowAsync<NotFoundException>().WithMessage("*Alighting station*");
    }

    [Fact]
    public async Task CreateBooking_BoardingAfterAlighting_ThrowsConflictException()
    {
        var (svc, db, train) = BuildFullService(nameof(CreateBooking_BoardingAfterAlighting_ThrowsConflictException));
        AddStops(db, train);

        var input = ValidInput(train.Id);
        input.BoardingStation = "City B";   // stop 3
        input.AlightingStation = "City A";  // stop 1

        await svc.Invoking(s => s.CreateBookingAsync(input))
            .Should().ThrowAsync<ConflictException>().WithMessage("*Boarding station must come before*");
    }

    [Fact]
    public async Task CreateBooking_SameBoardingAndAlighting_ThrowsValidationException()
    {
        var (svc, db, train) = BuildFullService(nameof(CreateBooking_SameBoardingAndAlighting_ThrowsValidationException));
        AddStops(db, train);

        var input = ValidInput(train.Id);
        input.BoardingStation = "City A";
        input.AlightingStation = "city a"; // case-insensitive match

        await svc.Invoking(s => s.CreateBookingAsync(input))
            .Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task PromoteWaitlist_PromotesFirstWaitlistedToConfirmed()
    {
        var (svc, db, train) = BuildFullService(nameof(PromoteWaitlist_PromotesFirstWaitlistedToConfirmed), availableSeats: 0);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var booking = await svc.CreateBookingAsync(ValidInput(train.Id));
        booking.Status.Should().Be("Waitlisted");

        await svc.PromoteWaitlistAsync(train.Id, today);

        db.ChangeTracker.Clear();
        var promoted = await db.Bookings.FindAsync(booking.Id);
        promoted!.Status.Should().Be("Confirmed");
        promoted.WaitlistPosition.Should().BeNull();
    }

    [Fact]
    public async Task PromoteWaitlist_RenumbersRemainingPositions()
    {
        var (svc, db, train) = BuildFullService(nameof(PromoteWaitlist_RenumbersRemainingPositions), availableSeats: 0);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await svc.CreateBookingAsync(ValidInput(train.Id));
        await svc.CreateBookingAsync(ValidInput(train.Id));
        await svc.CreateBookingAsync(ValidInput(train.Id));

        await svc.PromoteWaitlistAsync(train.Id, today);

        db.ChangeTracker.Clear();
        var remaining = await db.Bookings
            .Where(b => b.TrainId == train.Id && b.TravelDate == today && b.Status == "Waitlisted")
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync();
        remaining.Should().HaveCount(2);
        remaining[0].WaitlistPosition.Should().Be(1);
        remaining[1].WaitlistPosition.Should().Be(2);
    }

    [Fact]
    public async Task PromoteWaitlist_WhenNoWaitlist_DoesNothing()
    {
        var (svc, db, train) = BuildFullService(nameof(PromoteWaitlist_WhenNoWaitlist_DoesNothing), availableSeats: 10);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await svc.Invoking(s => s.PromoteWaitlistAsync(train.Id, today))
            .Should().NotThrowAsync();
        db.Bookings.Count().Should().Be(0);
    }

    [Fact]
    public async Task GetMyBookings_ReturnsAllForUser()
    {
        var (svc, _, train) = BuildFullService(nameof(GetMyBookings_ReturnsAllForUser), availableSeats: 10);
        var input1 = ValidInput(train.Id); input1.UserId = 1;
        var input2 = ValidInput(train.Id); input2.UserId = 1;
        var input3 = ValidInput(train.Id); input3.UserId = 2;
        await svc.CreateBookingAsync(input1);
        await svc.CreateBookingAsync(input2);
        await svc.CreateBookingAsync(input3);

        var results = await svc.GetMyBookingsAsync(1);

        results.Should().HaveCount(2);
        results.Should().OnlyContain(b => b.UserId == 1);
    }

    [Fact]
    public async Task GetMyBookings_ReturnsEmpty_WhenNone()
    {
        var (svc, _, _) = BuildFullService(nameof(GetMyBookings_ReturnsEmpty_WhenNone));

        var results = await svc.GetMyBookingsAsync(999);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBookingById_ReturnsEnrichedResponse_WithTrainName()
    {
        var (svc, _, train) = BuildFullService(nameof(GetBookingById_ReturnsEnrichedResponse_WithTrainName));
        var created = await svc.CreateBookingAsync(ValidInput(train.Id));

        var result = await svc.GetBookingByIdAsync(created.Id, created.UserId);

        result.Id.Should().Be(created.Id);
        result.TrainName.Should().Be("Test Express");
    }

    [Fact]
    public async Task GetBookingById_ThrowsNotFound_WhenMissing()
    {
        var (svc, _, _) = BuildFullService(nameof(GetBookingById_ThrowsNotFound_WhenMissing));

        await svc.Invoking(s => s.GetBookingByIdAsync(9999, 1))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBookingById_ThrowsUnauthorized_WhenWrongUser()
    {
        var (svc, _, train) = BuildFullService(nameof(GetBookingById_ThrowsUnauthorized_WhenWrongUser));
        var created = await svc.CreateBookingAsync(ValidInput(train.Id));

        await svc.Invoking(s => s.GetBookingByIdAsync(created.Id, 999))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authorized to view*");
    }

    [Fact]
    public async Task CancelBooking_ThrowsUnauthorized_WhenWrongUser()
    {
        var (svc, _, train) = BuildFullService(nameof(CancelBooking_ThrowsUnauthorized_WhenWrongUser));
        var created = await svc.CreateBookingAsync(ValidInput(train.Id));

        await svc.Invoking(s => s.CancelBookingAsync(created.Id, 999))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authorized to cancel*");
    }

    [Fact]
    public async Task CancelBooking_ThrowsConflict_WhenWithin2HoursOfDeparture()
    {
        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseInMemoryDatabase(nameof(CancelBooking_ThrowsConflict_WhenWithin2HoursOfDeparture))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new TrainDbContext(options);
        var seatRepo = new SeatAvailabilityRepository(db);
        var bookingRepo = new TrainBookingRepository(db);
        var trainRepo = new TrainRepository(db, NullLogger<TrainRepository>.Instance);

        var imminent = new Train
        {
            TrainName = "Imminent Express",
            TrainNumber = "IMM001",
            Source = "A",
            Destination = "B",
            DepartureTime = DateTime.UtcNow.AddMinutes(90),
            ArrivalTime = DateTime.UtcNow.AddHours(5),
            Price = 100m
        };
        db.Trains.Add(imminent);
        db.SaveChanges();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.SeatAvailabilities.Add(new SeatAvailability { TrainId = imminent.Id, Date = today, AvailableSeats = 10 });
        db.SaveChanges();

        var svc = new TrainBookingService(
            bookingRepo, seatRepo, trainRepo,
            new CreateTrainBookingInputValidator(),
            BuildMapper(), db,
            NullLogger<TrainBookingService>.Instance);

        var input = ValidInput(imminent.Id);
        input.UserId = 1;
        var created = await svc.CreateBookingAsync(input);

        await svc.Invoking(s => s.CancelBookingAsync(created.Id, 1))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*2 hours*");
    }

    [Fact]
    public async Task CancelBooking_ThrowsConflict_WhenAlreadyCancelled()
    {
        var (svc, _, train) = BuildFullService(nameof(CancelBooking_ThrowsConflict_WhenAlreadyCancelled));
        var created = await svc.CreateBookingAsync(ValidInput(train.Id));
        await svc.CancelBookingAsync(created.Id, created.UserId);

        await svc.Invoking(s => s.CancelBookingAsync(created.Id, created.UserId))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*already cancelled*");
    }
}
