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
        svc.Setup(s => s.GetAllTrainsAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync([MakeTrainResponse(), MakeTrainResponse()]);
        var controller = new TrainController(svc.Object);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<TrainResponse>>(ok.Value);
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingTrain_ReturnsOk()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.GetTrainByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeTrainResponse(1));
        var controller = new TrainController(svc.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.GetTrainByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TrainResponse?)null);
        var controller = new TrainController(svc.Object);

        var result = await controller.GetById(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidInput_Returns201()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.CreateTrainAsync(It.IsAny<CreateTrainInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeTrainResponse(1));
        var controller = new TrainController(svc.Object);
        var input = new CreateTrainInput { TrainName = "T", TrainNumber = "12345", Source = "A", Destination = "B", DepartureTime = DateTime.UtcNow.AddDays(1) };

        var result = await controller.Create(input, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Update_ValidInput_Returns200()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.UpdateTrainAsync(1, It.IsAny<UpdateTrainInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeTrainResponse(1));
        var controller = new TrainController(svc.Object);

        var result = await controller.Update(1, new UpdateTrainInput { TrainName = "Updated" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ExistingTrain_Returns204()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.DeleteTrainAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new TrainController(svc.Object);

        var result = await controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetSeatAvailability_ReturnsOk()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.GetSeatAvailabilityAsync(1, It.IsAny<CancellationToken>()))
           .ReturnsAsync([MakeSeatResponse()]);
        var controller = new TrainController(svc.Object);

        var result = await controller.GetSeatAvailability(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<List<SeatAvailabilityResponse>>(ok.Value);
    }

    [Fact]
    public async Task UpdateSeatAvailability_ReturnsOk()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.UpdateSeatAvailabilityAsync(1, It.IsAny<SeatAvailabilityInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeSeatResponse());
        var controller = new TrainController(svc.Object);
        var input = new SeatAvailabilityInput { Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 100 };

        var result = await controller.UpdateSeatAvailability(1, input, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
