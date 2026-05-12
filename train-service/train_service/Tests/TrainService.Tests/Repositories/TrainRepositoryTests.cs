using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TrainService.Core.Data;
using TrainService.Core.Models;
using TrainService.Core.Repositories;

namespace TrainService.Tests.Repositories;

[Collection("postgres")]
public class TrainRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private TrainDbContext _db = null!;
    private TrainRepository _repo = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
        _db = new TrainDbContext(options);
        _repo = new TrainRepository(_db, NullLogger<TrainRepository>.Instance);
        await _db.SeatAvailabilities.ExecuteDeleteAsync();
        await _db.TrainStops.ExecuteDeleteAsync();
        await _db.Trains.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    private static Train MakeTrain(string? number = null) => new()
    {
        TrainName = "Test Express",
        TrainNumber = number ?? $"T{Guid.NewGuid():N}"[..8],
        Source = "City A",
        Destination = "City B",
        DepartureTime = DateTime.UtcNow.AddDays(1),
        ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(10),
        Price = 500m
    };

    private static List<TrainStop> MakeStops(int trainId, IEnumerable<string> stations)
        => stations.Select((name, i) => new TrainStop { TrainId = trainId, StopNumber = i + 1, StationName = name }).ToList();

    [Fact]
    public async Task AddAsync_PersistsAndReturnsTrain()
    {
        var train = await _repo.AddAsync(MakeTrain("12345"));

        train.Id.Should().BeGreaterThan(0);
        train.TrainNumber.Should().Be("12345");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTrain_ReturnsTrain()
    {
        var created = await _repo.AddAsync(MakeTrain());

        var found = await _repo.GetByIdAsync(created.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(99999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByTrainNumberAsync_ExistingNumber_ReturnsTrain()
    {
        await _repo.AddAsync(MakeTrain("99999"));

        var found = await _repo.GetByTrainNumberAsync("99999");

        found.Should().NotBeNull();
        found!.TrainNumber.Should().Be("99999");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTrains()
    {
        await _repo.AddAsync(MakeTrain());
        await _repo.AddAsync(MakeTrain());

        var all = await _repo.GetAllAsync();

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var train = await _repo.AddAsync(MakeTrain());
        train.TrainName = "Updated Express";

        var updated = await _repo.UpdateAsync(train);

        updated.TrainName.Should().Be("Updated Express");
    }

    [Fact]
    public async Task DeleteAsync_RemovesTrain()
    {
        var train = await _repo.AddAsync(MakeTrain());

        await _repo.DeleteAsync(train.Id);

        var found = await _repo.GetByIdAsync(train.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task Query_ReturnsIQueryable()
    {
        await _repo.AddAsync(MakeTrain());
        await _repo.AddAsync(MakeTrain());

        var count = _repo.Query().Count();

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdWithStopsAsync_IncludesOrderedStops()
    {
        var train = await _repo.AddAsync(MakeTrain());
        _db.TrainStops.AddRange(MakeStops(train.Id, ["Vizag", "Vijayawada", "Secunderabad"]));
        await _db.SaveChangesAsync();

        var found = await _repo.GetByIdWithStopsAsync(train.Id);

        found.Should().NotBeNull();
        found!.Stops.Should().HaveCount(3);
        found.Stops.Select(s => s.StopNumber).Should().BeInAscendingOrder();
        found.Stops.First().StationName.Should().Be("Vizag");
    }

    [Fact]
    public async Task SearchByRoute_FindsTrainByIntermediateStop()
    {
        var train = await _repo.AddAsync(MakeTrain());
        _db.TrainStops.AddRange(MakeStops(train.Id, ["Vizag", "Vijayawada", "Warangal", "Secunderabad"]));
        _db.SeatAvailabilities.Add(new SeatAvailability { TrainId = train.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 100 });
        await _db.SaveChangesAsync();

        var results = await _repo.SearchByRouteAsync("Vijayawada", "Secunderabad", null, requiresAvailability: true);

        results.Should().HaveCount(1);
        results[0].Id.Should().Be(train.Id);
    }

    [Fact]
    public async Task SearchByRoute_RespectsDirection_ExcludesReverseTrains()
    {
        // Train goes Vizag → Secunderabad
        var trainA = await _repo.AddAsync(MakeTrain());
        _db.TrainStops.AddRange(MakeStops(trainA.Id, ["Vizag", "Vijayawada", "Secunderabad"]));
        _db.SeatAvailabilities.Add(new SeatAvailability { TrainId = trainA.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 100 });
        await _db.SaveChangesAsync();

        // Search Secunderabad → Vizag — should NOT match trainA since Secunderabad (stop 3) > Vizag (stop 1)
        var results = await _repo.SearchByRouteAsync("Secunderabad", "Vizag", null, requiresAvailability: true);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchByRoute_BothDirections_OnlyReturnsCorrectTrain()
    {
        // Train A: Vizag → Secunderabad
        var trainA = await _repo.AddAsync(MakeTrain());
        _db.TrainStops.AddRange(MakeStops(trainA.Id, ["Vizag", "Vijayawada", "Secunderabad"]));
        _db.SeatAvailabilities.Add(new SeatAvailability { TrainId = trainA.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 100 });

        // Train B: Secunderabad → Vizag
        var trainB = await _repo.AddAsync(MakeTrain());
        _db.TrainStops.AddRange(MakeStops(trainB.Id, ["Secunderabad", "Vijayawada", "Vizag"]));
        _db.SeatAvailabilities.Add(new SeatAvailability { TrainId = trainB.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 100 });

        await _db.SaveChangesAsync();

        var vizagToSec = await _repo.SearchByRouteAsync("Vizag", "Secunderabad", null, requiresAvailability: true);
        var secToVizag = await _repo.SearchByRouteAsync("Secunderabad", "Vizag", null, requiresAvailability: true);

        vizagToSec.Should().HaveCount(1).And.Contain(t => t.Id == trainA.Id);
        secToVizag.Should().HaveCount(1).And.Contain(t => t.Id == trainB.Id);
    }
}
