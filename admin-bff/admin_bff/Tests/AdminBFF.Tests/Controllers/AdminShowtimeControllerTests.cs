using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using AdminBFF.Endpoints.Controllers;

namespace AdminBFF.Tests.Controllers;

public class AdminShowtimeControllerTests
{
    private static ShowtimeDto MakeShowtime(int id = 1) => new()
    {
        Id = id,
        MovieId = 1,
        ShowDate = "2026-12-25",
        ShowTime = "14:30",
        ScreenNumber = "Screen 1",
        TotalSeats = 50,
        AvailableSeats = 50,
        CreatedAt = DateTime.UtcNow.ToString("O")
    };

    [Fact]
    public async Task GetShowtimes_Returns200WithList()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.GetShowtimesAsync(1, It.IsAny<CancellationToken>()))
           .ReturnsAsync([MakeShowtime(1), MakeShowtime(2)]);
        var controller = new AdminShowtimeController(svc.Object);

        var result = await controller.GetShowtimes(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<ShowtimeDto>>(ok.Value);
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateShowtime_Returns201()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.CreateShowtimeAsync(It.IsAny<CreateShowtimeRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeShowtime(1));
        var controller = new AdminShowtimeController(svc.Object);
        var request = new CreateShowtimeRequest { ShowDate = "2026-12-25", ShowTime = "14:30", ScreenNumber = "Screen 1", TotalSeats = 50 };

        var result = await controller.CreateShowtime(1, request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task DeleteShowtime_Returns204()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.DeleteShowtimeAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new AdminShowtimeController(svc.Object);

        var result = await controller.DeleteShowtime(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
