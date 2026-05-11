using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using AdminBFF.Endpoints.Controllers;

namespace AdminBFF.Tests.Controllers;

public class AdminTrainControllerTests
{
    private static readonly Faker Fake = new();

    private static TrainDto MakeTrain(int id = 0) => new()
    {
        Id = id > 0 ? id : Fake.Random.Int(1, 1000),
        TrainName = "Rajdhani",
        TrainNumber = "12301",
        Source = "Delhi",
        Destination = "Howrah",
        DepartureTime = DateTime.UtcNow.AddDays(1)
    };

    [Fact]
    public async Task Create_Returns201()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.CreateTrainAsync(It.IsAny<CreateTrainRequest>()))
           .ReturnsAsync(MakeTrain(1));
        var controller = new AdminTrainController(svc.Object);

        var result = await controller.Create(new CreateTrainRequest { TrainName = "T", TrainNumber = "1", Source = "A", Destination = "B" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Update_Returns200()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.UpdateTrainAsync(1, It.IsAny<UpdateTrainRequest>()))
           .ReturnsAsync(MakeTrain(1));
        var controller = new AdminTrainController(svc.Object);

        var result = await controller.Update(1, new UpdateTrainRequest { TrainName = "Updated" });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.DeleteTrainAsync(1)).Returns(Task.CompletedTask);
        var controller = new AdminTrainController(svc.Object);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetSeatAvailability_Returns200()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.GetSeatAvailabilityAsync(1))
           .ReturnsAsync([new SeatAvailabilityDto { Id = 1, TrainId = 1, Date = "2025-06-01", AvailableSeats = 100 }]);
        var controller = new AdminTrainController(svc.Object);

        var result = await controller.GetSeatAvailability(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<SeatAvailabilityDto>>(ok.Value);
        list.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateSeatAvailability_Returns200()
    {
        var svc = new Mock<ITrainService>();
        svc.Setup(s => s.UpdateSeatAvailabilityAsync(1, It.IsAny<UpdateSeatAvailabilityRequest>()))
           .ReturnsAsync(new SeatAvailabilityDto { Id = 1, TrainId = 1, Date = "2025-06-01", AvailableSeats = 50 });
        var controller = new AdminTrainController(svc.Object);

        var result = await controller.UpdateSeatAvailability(1, new UpdateSeatAvailabilityRequest { Date = "2025-06-01", AvailableSeats = 50 });

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
