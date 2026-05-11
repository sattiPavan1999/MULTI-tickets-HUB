using AutoMapper;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TrainService.Core.Data;
using TrainService.Core.DTOs;
using TrainService.Core.Exceptions;
using TrainService.Core.Mapping;
using TrainService.Core.Models;
using TrainService.Core.Repositories;
using TrainService.Core.Services;
using TrainService.Core.Validators;

namespace TrainService.Tests.Services;

public class TrainServiceTests
{
    private static readonly Faker Fake = new();

    private static IMapper BuildMapper()
        => new MapperConfiguration(c => c.AddProfile<TrainMappingProfile>()).CreateMapper();

    private static (ITrainService svc, TrainDbContext db) BuildFullService(string dbName)
    {
        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new TrainDbContext(options);
        var trainRepo = new TrainRepository(db, NullLogger<TrainRepository>.Instance);
        var seatRepo = new SeatAvailabilityRepository(db);
        var svc = new TrainService.Core.Services.TrainService(
            trainRepo, seatRepo,
            new CreateTrainInputValidator(),
            new UpdateTrainInputValidator(),
            new SeatAvailabilityInputValidator(),
            BuildMapper(),
            NullLogger<TrainService.Core.Services.TrainService>.Instance);
        return (svc, db);
    }

    private static CreateTrainInput ValidCreateInput(string? number = null) => new()
    {
        TrainName = "Rajdhani Express",
        TrainNumber = number ?? $"T{Fake.Random.Number(1000, 9999)}",
        Source = "New Delhi",
        Destination = "Howrah",
        DepartureTime = DateTime.UtcNow.AddDays(1),
        ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(18),
        Price = 1200m
    };

    [Fact]
    public async Task CreateTrain_ValidInput_ReturnsTrainResponse()
    {
        var (svc, _) = BuildFullService(nameof(CreateTrain_ValidInput_ReturnsTrainResponse));

        var result = await svc.CreateTrainAsync(ValidCreateInput("12301"));

        result.TrainNumber.Should().Be("12301");
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateTrain_DuplicateNumber_ThrowsConflictException()
    {
        var (svc, _) = BuildFullService(nameof(CreateTrain_DuplicateNumber_ThrowsConflictException));
        await svc.CreateTrainAsync(ValidCreateInput("99999"));

        await svc.Invoking(s => s.CreateTrainAsync(ValidCreateInput("99999")))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateTrain_EmptyTrainName_ThrowsValidationException()
    {
        var (svc, _) = BuildFullService(nameof(CreateTrain_EmptyTrainName_ThrowsValidationException));
        var input = ValidCreateInput();
        input.TrainName = "";

        await svc.Invoking(s => s.CreateTrainAsync(input))
            .Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task GetAllTrains_ReturnsAll()
    {
        var trainRepo = new Mock<ITrainRepository>();
        var seatRepo = new Mock<ISeatAvailabilityRepository>();
        trainRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Train { Id = 1, TrainName = "T", TrainNumber = "1", Source = "A", Destination = "B", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(5), Price = 500m }]);
        var svc = new TrainService.Core.Services.TrainService(
            trainRepo.Object, seatRepo.Object,
            new CreateTrainInputValidator(), new UpdateTrainInputValidator(), new SeatAvailabilityInputValidator(),
            BuildMapper(), NullLogger<TrainService.Core.Services.TrainService>.Instance);

        var result = await svc.GetAllTrainsAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateTrain_UpdatesName()
    {
        var (svc, _) = BuildFullService(nameof(UpdateTrain_UpdatesName));
        var created = await svc.CreateTrainAsync(ValidCreateInput());

        var result = await svc.UpdateTrainAsync(created.Id, new UpdateTrainInput { TrainName = "Updated Express" });

        result.TrainName.Should().Be("Updated Express");
    }

    [Fact]
    public async Task DeleteTrain_UnknownId_ThrowsNotFoundException()
    {
        var (svc, _) = BuildFullService(nameof(DeleteTrain_UnknownId_ThrowsNotFoundException));

        await svc.Invoking(s => s.DeleteTrainAsync(9999))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateSeatAvailability_ValidInput_ReturnsResponse()
    {
        var (svc, _) = BuildFullService(nameof(UpdateSeatAvailability_ValidInput_ReturnsResponse));
        var train = await svc.CreateTrainAsync(ValidCreateInput());
        var input = new SeatAvailabilityInput { Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 150 };

        var result = await svc.UpdateSeatAvailabilityAsync(train.Id, input);

        result.AvailableSeats.Should().Be(150);
        result.TrainId.Should().Be(train.Id);
    }

    [Fact]
    public async Task UpdateSeatAvailability_NegativeSeats_ThrowsValidationException()
    {
        var (svc, _) = BuildFullService(nameof(UpdateSeatAvailability_NegativeSeats_ThrowsValidationException));
        var train = await svc.CreateTrainAsync(ValidCreateInput());

        await svc.Invoking(s => s.UpdateSeatAvailabilityAsync(train.Id, new SeatAvailabilityInput { Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = -1 }))
            .Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task SearchTrains_NoParams_ReturnsAll()
    {
        var (svc, _) = BuildFullService(nameof(SearchTrains_NoParams_ReturnsAll));
        await svc.CreateTrainAsync(ValidCreateInput());
        await svc.CreateTrainAsync(ValidCreateInput());

        var result = await svc.SearchTrainsAsync(null, null, null);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchTrains_FilterBySource_ReturnsMatching()
    {
        var (svc, _) = BuildFullService(nameof(SearchTrains_FilterBySource_ReturnsMatching));
        await svc.CreateTrainAsync(new CreateTrainInput { TrainName = "Express A", TrainNumber = "SA1", Source = "New Delhi", Destination = "Howrah", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(2), Price = 1000m });
        await svc.CreateTrainAsync(new CreateTrainInput { TrainName = "Express B", TrainNumber = "SB1", Source = "Mumbai CST", Destination = "Pune", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(4), Price = 500m });

        var result = await svc.SearchTrainsAsync("New Delhi", null, null);

        result.Should().HaveCount(1);
        result[0].Source.Should().Be("New Delhi");
    }

    [Fact]
    public async Task SearchTrains_FilterByDestination_ReturnsMatching()
    {
        var (svc, _) = BuildFullService(nameof(SearchTrains_FilterByDestination_ReturnsMatching));
        await svc.CreateTrainAsync(new CreateTrainInput { TrainName = "Express A", TrainNumber = "DA1", Source = "Delhi", Destination = "Howrah", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(2), Price = 1000m });
        await svc.CreateTrainAsync(new CreateTrainInput { TrainName = "Express B", TrainNumber = "DB1", Source = "Delhi", Destination = "Bhopal", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(6), Price = 600m });

        var result = await svc.SearchTrainsAsync(null, "Howrah", null);

        result.Should().HaveCount(1);
        result[0].Destination.Should().Be("Howrah");
    }

    [Fact]
    public async Task SearchTrains_FilterBothSourceAndDestination_ReturnsExactMatch()
    {
        var (svc, _) = BuildFullService(nameof(SearchTrains_FilterBothSourceAndDestination_ReturnsExactMatch));
        await svc.CreateTrainAsync(new CreateTrainInput { TrainName = "Express A", TrainNumber = "BD1", Source = "New Delhi", Destination = "Howrah", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(2), Price = 1000m });
        await svc.CreateTrainAsync(new CreateTrainInput { TrainName = "Express B", TrainNumber = "BD2", Source = "New Delhi", Destination = "Bhopal", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(6), Price = 600m });
        await svc.CreateTrainAsync(new CreateTrainInput { TrainName = "Express C", TrainNumber = "BD3", Source = "Mumbai CST", Destination = "Howrah", DepartureTime = DateTime.UtcNow.AddDays(2), ArrivalTime = DateTime.UtcNow.AddDays(3), Price = 1200m });

        var result = await svc.SearchTrainsAsync("New Delhi", "Howrah", null);

        result.Should().HaveCount(1);
        result[0].TrainNumber.Should().Be("BD1");
    }
}
