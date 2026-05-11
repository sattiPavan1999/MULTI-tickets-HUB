using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieService.Core.DTOs;
using MovieService.Core.Services;
using MovieService.Endpoints.Controllers;

namespace MovieService.Tests.Controllers;

public class BookingControllerTests
{
    private static BookingResponse MakeResponse() => new()
    {
        Id = 1,
        ShowtimeId = 1,
        UserId = 42,
        SeatNumbers = "1,2,3",
        NumberOfSeats = 3,
        Status = "Pending",
        BookedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Create_ValidInput_Returns201()
    {
        var svc = new Mock<IBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateBookingInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeResponse());
        var controller = new BookingController(svc.Object);
        var input = new CreateBookingInput { ShowtimeId = 1, UserId = 42, SeatNumbers = [1, 2, 3] };

        var result = await controller.Create(input, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_CallsBookingService()
    {
        var svc = new Mock<IBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateBookingInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeResponse());
        var controller = new BookingController(svc.Object);
        var input = new CreateBookingInput { ShowtimeId = 1, UserId = 42, SeatNumbers = [1, 2, 3] };

        await controller.Create(input, CancellationToken.None);

        svc.Verify(s => s.CreateBookingAsync(input, CancellationToken.None), Times.Once);
    }
}
