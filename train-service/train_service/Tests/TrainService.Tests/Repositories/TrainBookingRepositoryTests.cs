using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TrainService.Core.Data;
using TrainService.Core.Models;
using TrainService.Core.Repositories;

namespace TrainService.Tests.Repositories;

[Collection("postgres")]
public class TrainBookingRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private TrainDbContext _db = null!;
    private TrainRepository _trainRepo = null!;
    private TrainBookingRepository _bookingRepo = null!;
    private Train _train = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
        _db = new TrainDbContext(options);
        _trainRepo = new TrainRepository(_db, NullLogger<TrainRepository>.Instance);
        _bookingRepo = new TrainBookingRepository(_db);

        await _db.Bookings.ExecuteDeleteAsync();
        await _db.SeatAvailabilities.ExecuteDeleteAsync();
        await _db.Trains.ExecuteDeleteAsync();

        _train = await _trainRepo.AddAsync(new Train
        {
            TrainName = "Booking Test Express",
            TrainNumber = $"BT{Guid.NewGuid():N}"[..8],
            Source = "A",
            Destination = "B",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(8),
            Price = 500m
        });
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    private TrainBooking MakeBooking(string status = "Confirmed", int? waitlistPosition = null) => new()
    {
        TrainId = _train.Id,
        UserId = 1,
        TravelDate = DateOnly.FromDateTime(DateTime.UtcNow),
        PassengerName = "Test Passenger",
        PassengerAge = 30,
        NumberOfSeats = 1,
        PNR = "PNR" + Guid.NewGuid().ToString("N").ToUpper()[..8],
        Status = status,
        WaitlistPosition = waitlistPosition,
        BookedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task AddAsync_Persists()
    {
        var booking = await _bookingRepo.AddAsync(MakeBooking());

        booking.Id.Should().BeGreaterThan(0);
        booking.PNR.Should().StartWith("PNR");
    }

    [Fact]
    public async Task GetByPNRAsync_Finds()
    {
        var booking = await _bookingRepo.AddAsync(MakeBooking());

        var found = await _bookingRepo.GetByPNRAsync(booking.PNR);

        found.Should().NotBeNull();
        found!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task GetWaitlistedByTrainAndDateAsync_ReturnsOrdered()
    {
        await _bookingRepo.AddAsync(MakeBooking("Waitlisted", 2));
        await _bookingRepo.AddAsync(MakeBooking("Waitlisted", 1));
        await _bookingRepo.AddAsync(MakeBooking("Confirmed", null));

        var result = await _bookingRepo.GetWaitlistedByTrainAndDateAsync(
            _train.Id, DateOnly.FromDateTime(DateTime.UtcNow));

        result.Should().HaveCount(2);
        result[0].WaitlistPosition.Should().Be(1);
        result[1].WaitlistPosition.Should().Be(2);
    }
}
