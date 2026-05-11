using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrainService.Core.DTOs;
using TrainService.Core.Services;
using TrainService.Endpoints.Controllers;

namespace TrainService.Tests.Controllers;

public class TrainControllerTests
{
    private static readonly Faker Fake = new();

    private static TrainResponse MakeTrainResponse(int id = 0) => new()
    {
        Id = id > 0 ? id : Fake.Random.Int(1, 1000),
        TrainName = "Rajdhani Express",
        TrainNumber = "12301",
        Source = "New Delhi",
        Destination = "Howrah",
        DepartureTime = DateTime.UtcNow.AddDays(1),
        ArrivalTime = DateTime.UtcNow.AddDays(2),
        Price = 1200m,
        CreatedAt = DateTime.UtcNow
    };

    private static SeatAvailabilityResponse MakeSeatResponse() => new()
    {
        Id = 1,
        TrainId = 1,
        Date = DateOnly.FromDateTime(DateTime.UtcNow),
        AvailableSeats = 100
    };

    [Fact]
    public async Task GetAll_ReturnsOkWithTrains()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.SearchTrainsAsync(null, null, null, false))
           .ReturnsAsync([MakeTrainResponse(), MakeTrainResponse()]);
        var controller = new TrainController(svc.Object);

        var result = await controller.GetAll(null, null, null, false);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<TrainResponse>>(ok.Value);
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithSearchParams_PassesThemToService()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.SearchTrainsAsync("New Delhi", "Howrah", "price", false))
           .ReturnsAsync([MakeTrainResponse()]);
        var controller = new TrainController(svc.Object);

        var result = await controller.GetAll("New Delhi", "Howrah", "price", false);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        svc.Verify(s => s.SearchTrainsAsync("New Delhi", "Howrah", "price", false), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingTrain_ReturnsOk()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.GetTrainByIdAsync(1)).ReturnsAsync(MakeTrainResponse(1));
        var controller = new TrainController(svc.Object);

        var result = await controller.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.GetTrainByIdAsync(99)).ReturnsAsync((TrainResponse?)null);
        var controller = new TrainController(svc.Object);

        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidInput_Returns201()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.CreateTrainAsync(It.IsAny<CreateTrainInput>()))
           .ReturnsAsync(MakeTrainResponse(1));
        var controller = new TrainController(svc.Object);
        var input = new CreateTrainInput { TrainName = "T", TrainNumber = "12345", Source = "A", Destination = "B", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(5), Price = 500m };

        var result = await controller.Create(input);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Update_ValidInput_Returns200()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.UpdateTrainAsync(1, It.IsAny<UpdateTrainInput>()))
           .ReturnsAsync(MakeTrainResponse(1));
        var controller = new TrainController(svc.Object);

        var result = await controller.Update(1, new UpdateTrainInput { TrainName = "Updated" });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ExistingTrain_Returns204()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.DeleteTrainAsync(1)).Returns(Task.CompletedTask);
        var controller = new TrainController(svc.Object);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetSeatAvailability_ReturnsOk()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.GetSeatAvailabilityAsync(1))
           .ReturnsAsync([MakeSeatResponse()]);
        var controller = new TrainController(svc.Object);

        var result = await controller.GetSeatAvailability(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<List<SeatAvailabilityResponse>>(ok.Value);
    }

    [Fact]
    public async Task UpdateSeatAvailability_ReturnsOk()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.UpdateSeatAvailabilityAsync(1, It.IsAny<SeatAvailabilityInput>()))
           .ReturnsAsync(MakeSeatResponse());
        var controller = new TrainController(svc.Object);
        var input = new SeatAvailabilityInput { Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 100 };

        var result = await controller.UpdateSeatAvailability(1, input);

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
