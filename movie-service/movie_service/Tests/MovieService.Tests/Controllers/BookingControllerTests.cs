using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieService.Core.DTOs;
using MovieService.Core.Services;
using MovieService.Endpoints.Controllers;

namespace MovieService.Tests.Controllers;

public class BookingControllerTests
{
    private static BookingController BuildController(IBookingService svc, int userId = 42)
    {
        var controller = new BookingController(svc);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Id"] = userId.ToString();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static BookingResponse MakeResponse() => new()
    {
        Id = 1,
        ShowtimeId = 1,
        UserId = 42,
        SeatNumbers = "1,2,3",
        NumberOfSeats = 3,
        Status = "Confirmed",
        BookedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Create_ValidInput_Returns201()
    {
        var svc = new Mock<IBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateBookingInput>()))
           .ReturnsAsync(MakeResponse());
        var controller = BuildController(svc.Object, userId: 42);
        var input = new CreateBookingInput { ShowtimeId = 1, SeatNumbers = [1, 2, 3] };

        var result = await controller.Create(input);

        var created = Assert.IsType<ObjectResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_MissingUserIdHeader_Returns401()
    {
        var svc = new Mock<IBookingService>();
        var controller = new BookingController(svc.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var input = new CreateBookingInput { ShowtimeId = 1, SeatNumbers = [1, 2, 3] };

        var result = await controller.Create(input);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Create_CallsBookingServiceWithHeaderUserId()
    {
        var svc = new Mock<IBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateBookingInput>()))
           .ReturnsAsync(MakeResponse());
        var controller = BuildController(svc.Object, userId: 42);
        var input = new CreateBookingInput { ShowtimeId = 1, SeatNumbers = [1, 2, 3] };

        await controller.Create(input);

        svc.Verify(s => s.CreateBookingAsync(It.Is<CreateBookingInput>(i => i.UserId == 42)), Times.Once);
    }
}
