using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrainService.Core.DTOs;
using TrainService.Core.Services;
using TrainService.Endpoints.Controllers;

namespace TrainService.Tests.Controllers;

public class BookingControllerTests
{
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
        var controller = new BookingController(svc.Object);
        var input = new CreateTrainBookingInput { TrainId = 1, UserId = 1, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), PassengerName = "Alice", PassengerAge = 28, NumberOfSeats = 2 };

        var result = await controller.Create(input, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_CallsBookingService()
    {
        var svc = new Mock<ITrainBookingService>();
        svc.Setup(s => s.CreateBookingAsync(It.IsAny<CreateTrainBookingInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeResponse());
        var controller = new BookingController(svc.Object);
        var input = new CreateTrainBookingInput { TrainId = 1, UserId = 1, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"), PassengerName = "Alice", PassengerAge = 28, NumberOfSeats = 2 };

        await controller.Create(input, CancellationToken.None);

        svc.Verify(s => s.CreateBookingAsync(input, CancellationToken.None), Times.Once);
    }
}
