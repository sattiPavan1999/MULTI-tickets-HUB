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
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateTrainBookingInput>()))
           .ReturnsAsync(MakeResponse());
        var controller = BuildController(svc.Object, userId: 1);
        var input = new CreateTrainBookingInput { TrainId = 1, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), PassengerName = "Alice", PassengerAge = 28, NumberOfSeats = 2 };

        var result = await controller.Create(input);

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

        var result = await controller.Create(input);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Create_CallsBookingServiceWithHeaderUserId()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateTrainBookingInput>()))
           .ReturnsAsync(MakeResponse());
        var controller = BuildController(svc.Object, userId: 1);
        var input = new CreateTrainBookingInput { TrainId = 1, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), PassengerName = "Alice", PassengerAge = 28, NumberOfSeats = 2 };

        await controller.Create(input);

        svc.Verify(s => s.CreateBookingAsync(It.Is<CreateTrainBookingInput>(i => i.UserId == 1)), Times.Once);
    }

    [Fact]
    public async Task Cancel_ValidHeader_CallsServiceWithBothArgs()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.CancelBookingAsync(It.IsAny<int>(), It.IsAny<int>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "Booking cancelled successfully" });
        var controller = BuildController(svc.Object, userId: 1);

        var result = await controller.Cancel(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        ok.StatusCode.Should().Be(200);
        svc.Verify(s => s.CancelBookingAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task Cancel_MissingHeader_Returns401()
    {
        var svc = new Mock<ITrainBookingService>();
        var controller = new BookingController(svc.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.Cancel(1);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetMyBookings_ValidHeader_Returns200()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.GetMyBookingsAsync(It.IsAny<int>()))
           .ReturnsAsync([MakeResponse()]);
        var controller = BuildController(svc.Object, userId: 1);

        var result = await controller.GetMyBookings();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        ok.StatusCode.Should().Be(200);
        svc.Verify(s => s.GetMyBookingsAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetMyBookings_MissingHeader_Returns401()
    {
        var svc = new Mock<ITrainBookingService>();
        var controller = new BookingController(svc.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.GetMyBookings();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ValidHeader_Returns200()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.GetBookingByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
           .ReturnsAsync(MakeResponse());
        var controller = BuildController(svc.Object, userId: 1);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        ok.StatusCode.Should().Be(200);
        svc.Verify(s => s.GetBookingByIdAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task GetById_MissingHeader_Returns401()
    {
        var svc = new Mock<ITrainBookingService>();
        var controller = new BookingController(svc.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.GetById(1);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }
}
