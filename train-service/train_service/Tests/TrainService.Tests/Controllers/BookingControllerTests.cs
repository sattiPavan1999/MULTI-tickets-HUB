using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrainService.Core.DTOs;
using TrainService.Core.Services;
using TrainService.Endpoints.Controllers;

namespace TrainService.Tests.Controllers;

public class BookingControllerTests
{
    private static BookingController BuildController(ITrainBookingService svc, int userId = 1)
    {
        var controller = new BookingController(svc);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Id"] = userId.ToString();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static TrainBookingResponse MakeResponse() => new()
    {
        Id = 1,
        TrainId = 1,
        UserId = 1,
        TravelDate = DateOnly.FromDateTime(DateTime.UtcNow),
        PassengerName = "Alice",
        PassengerAge = 28,
        NumberOfSeats = 2,
        PNR = "PNRABC12345",
        Status = "Confirmed",
        WaitlistPosition = null,
        BookedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Create_ValidInput_Returns201()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateTrainBookingInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeResponse());
        var controller = BuildController(svc.Object, userId: 1);
        var input = new CreateTrainBookingInput { TrainId = 1, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), PassengerName = "Alice", PassengerAge = 28, NumberOfSeats = 2 };

        var result = await controller.Create(input, CancellationToken.None);

        var created = Assert.IsType<ObjectResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_MissingUserIdHeader_Returns401()
    {
        var svc = new Mock<ITrainBookingService>();
        var controller = new BookingController(svc.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var input = new CreateTrainBookingInput { TrainId = 1, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), PassengerName = "Alice", PassengerAge = 28, NumberOfSeats = 2 };

        var result = await controller.Create(input, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Create_CallsBookingServiceWithHeaderUserId()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateTrainBookingInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeResponse());
        var controller = BuildController(svc.Object, userId: 1);
        var input = new CreateTrainBookingInput { TrainId = 1, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), PassengerName = "Alice", PassengerAge = 28, NumberOfSeats = 2 };

        await controller.Create(input, CancellationToken.None);

        svc.Verify(s => s.CreateBookingAsync(It.Is<CreateTrainBookingInput>(i => i.UserId == 1), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Cancel_CallsCancelBookingService()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.CancelBookingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "Booking cancelled successfully" });
        var controller = BuildController(svc.Object);

        var result = await controller.Cancel(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        ok.StatusCode.Should().Be(200);
        svc.Verify(s => s.CancelBookingAsync(1, CancellationToken.None), Times.Once);
    }
}
