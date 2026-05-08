using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TrainService.Core.Data;
using TrainService.Core.Models;
using TrainService.Core.Repositories;

namespace TrainService.Tests.Repositories;

[Collection("postgres")]
public class SeatAvailabilityRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private TrainDbContext _db = null!;
    private TrainRepository _trainRepo = null!;
    private SeatAvailabilityRepository _seatRepo = null!;
    private Train _train = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
        _db = new TrainDbContext(options);
        _trainRepo = new TrainRepository(_db, NullLogger<TrainRepository>.Instance);
        _seatRepo = new SeatAvailabilityRepository(_db);

        await _db.SeatAvailabilities.ExecuteDeleteAsync();
        await _db.Trains.ExecuteDeleteAsync();

        _train = await _trainRepo.AddAsync(new Train
        {
            TrainName = "Seat Test Express",
            TrainNumber = $"ST{Guid.NewGuid():N}"[..8],
            Source = "A",
            Destination = "B",
            DepartureTime = DateTime.UtcNow.AddDays(1)
        }, CancellationToken.None);
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    [Fact]
    public async Task UpsertAsync_Insert_CreatesNewEntry()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var availability = new SeatAvailability { TrainId = _train.Id, Date = date, AvailableSeats = 100 };

        var result = await _seatRepo.UpsertAsync(availability, CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.AvailableSeats.Should().Be(100);
    }

    [Fact]
    public async Task UpsertAsync_Update_ModifiesExistingEntry()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6));
        await _seatRepo.UpsertAsync(new SeatAvailability { TrainId = _train.Id, Date = date, AvailableSeats = 100 }, CancellationToken.None);

        var updated = await _seatRepo.UpsertAsync(new SeatAvailability { TrainId = _train.Id, Date = date, AvailableSeats = 50 }, CancellationToken.None);

        updated.AvailableSeats.Should().Be(50);
        var all = await _seatRepo.GetByTrainAsync(_train.Id, CancellationToken.None);
        all.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByTrainAsync_ReturnsEntriesForTrain()
    {
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var date2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8));
        await _seatRepo.UpsertAsync(new SeatAvailability { TrainId = _train.Id, Date = date1, AvailableSeats = 100 }, CancellationToken.None);
        await _seatRepo.UpsertAsync(new SeatAvailability { TrainId = _train.Id, Date = date2, AvailableSeats = 80 }, CancellationToken.None);

        var all = await _seatRepo.GetByTrainAsync(_train.Id, CancellationToken.None);

        all.Should().HaveCount(2);
        all.Should().AllSatisfy(s => s.TrainId.Should().Be(_train.Id));
    }

    [Fact]
    public async Task GetByTrainAndDateAsync_ExistingEntry_ReturnsIt()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(9));
        await _seatRepo.UpsertAsync(new SeatAvailability { TrainId = _train.Id, Date = date, AvailableSeats = 200 }, CancellationToken.None);

        var found = await _seatRepo.GetByTrainAndDateAsync(_train.Id, date, CancellationToken.None);

        found.Should().NotBeNull();
        found!.AvailableSeats.Should().Be(200);
    }

    [Fact]
    public async Task GetByTrainAndDateAsync_NonExistentEntry_ReturnsNull()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100));

        var found = await _seatRepo.GetByTrainAndDateAsync(_train.Id, date, CancellationToken.None);

        found.Should().BeNull();
    }
}
