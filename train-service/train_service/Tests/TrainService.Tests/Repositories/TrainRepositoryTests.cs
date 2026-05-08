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
        await _db.Trains.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    private static Train MakeTrain(string? number = null) => new()
    {
        TrainName = "Test Express",
        TrainNumber = number ?? $"T{Guid.NewGuid():N}"[..8],
        Source = "City A",
        Destination = "City B",
        DepartureTime = DateTime.UtcNow.AddDays(1)
    };

    [Fact]
    public async Task AddAsync_PersistsAndReturnsTrain()
    {
        var train = await _repo.AddAsync(MakeTrain("12345"), CancellationToken.None);

        train.Id.Should().BeGreaterThan(0);
        train.TrainNumber.Should().Be("12345");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTrain_ReturnsTrain()
    {
        var created = await _repo.AddAsync(MakeTrain(), CancellationToken.None);

        var found = await _repo.GetByIdAsync(created.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(99999, CancellationToken.None);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByTrainNumberAsync_ExistingNumber_ReturnsTrain()
    {
        await _repo.AddAsync(MakeTrain("99999"), CancellationToken.None);

        var found = await _repo.GetByTrainNumberAsync("99999", CancellationToken.None);

        found.Should().NotBeNull();
        found!.TrainNumber.Should().Be("99999");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTrains()
    {
        await _repo.AddAsync(MakeTrain(), CancellationToken.None);
        await _repo.AddAsync(MakeTrain(), CancellationToken.None);

        var all = await _repo.GetAllAsync(CancellationToken.None);

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var train = await _repo.AddAsync(MakeTrain(), CancellationToken.None);
        train.TrainName = "Updated Express";

        var updated = await _repo.UpdateAsync(train, CancellationToken.None);

        updated.TrainName.Should().Be("Updated Express");
    }

    [Fact]
    public async Task DeleteAsync_RemovesTrain()
    {
        var train = await _repo.AddAsync(MakeTrain(), CancellationToken.None);

        await _repo.DeleteAsync(train.Id, CancellationToken.None);

        var found = await _repo.GetByIdAsync(train.Id, CancellationToken.None);
        found.Should().BeNull();
    }

    [Fact]
    public async Task Query_ReturnsIQueryable()
    {
        await _repo.AddAsync(MakeTrain(), CancellationToken.None);
        await _repo.AddAsync(MakeTrain(), CancellationToken.None);

        var count = _repo.Query().Count();

        count.Should().Be(2);
    }
}
