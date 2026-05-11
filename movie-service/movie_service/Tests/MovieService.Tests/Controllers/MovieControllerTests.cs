using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieService.Core.DTOs;
using MovieService.Core.Services;
using MovieService.Endpoints.Controllers;

namespace MovieService.Tests.Controllers;

public class MovieControllerTests
{
    private static readonly Faker Fake = new();

    private static ShowtimeResponse MakeShowtimeResponse(int id = 0) => new()
    {
        Id = id > 0 ? id : Fake.Random.Int(1, 1000),
        MovieId = 1,
        ShowDate = new DateOnly(2026, 12, 25),
        ShowTime = new TimeOnly(14, 30),
        ScreenNumber = "Screen 1",
        TotalSeats = 50,
        AvailableSeats = 50,
        CreatedAt = DateTime.UtcNow
    };

    private static MovieResponse MakeResponse(int id = 0) => new()
    {
        Id = id > 0 ? id : Fake.Random.Int(1, 1000),
        Title = Fake.Lorem.Word(),
        Genre = "Action",
        Duration = 120,
        PosterUrl = "https://example.com/poster.jpg",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkWithMovies()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.GetAllMoviesAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync([MakeResponse(), MakeResponse()]);
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        var result = await controller.GetAll(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<MovieResponse>>(ok.Value);
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ActiveOnly_PassesFlagToService()
    {
        var active = MakeResponse();
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.GetAllMoviesAsync(true, It.IsAny<CancellationToken>()))
           .ReturnsAsync([active]);
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        var result = await controller.GetAll(activeOnly: true, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<MovieResponse>>(ok.Value);
        list.Should().HaveCount(1);
        svc.Verify(s => s.GetAllMoviesAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingMovie_ReturnsOk()
    {
        var movie = MakeResponse(1);
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.GetMovieByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<MovieResponse>(ok.Value);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.GetMovieByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((MovieResponse?)null);
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        var result = await controller.GetById(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidInput_Returns201WithMovie()
    {
        var created = MakeResponse(1);
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.CreateMovieAsync(It.IsAny<CreateMovieInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(created);
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);
        var input = new CreateMovieInput { Title = "Test", Genre = "Action", Duration = 120, PosterUrl = "https://example.com/p.jpg" };

        var result = await controller.Create(input, CancellationToken.None);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        createdAt.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_CallsService_WithInput()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.CreateMovieAsync(It.IsAny<CreateMovieInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeResponse());
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);
        var input = new CreateMovieInput { Title = "T", Genre = "G", Duration = 100, PosterUrl = "https://example.com/p.jpg" };

        await controller.Create(input, CancellationToken.None);

        svc.Verify(s => s.CreateMovieAsync(input, CancellationToken.None), Times.Once);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ValidInput_Returns200()
    {
        var updated = MakeResponse(1);
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.UpdateMovieAsync(1, It.IsAny<UpdateMovieInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(updated);
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        var result = await controller.Update(1, new UpdateMovieInput { Title = "New" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<MovieResponse>(ok.Value);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingMovie_Returns204()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.DeleteMovieAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        var result = await controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    // ── ToggleStatus ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleStatus_Returns200WithResult()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.ToggleMovieStatusAsync(1, It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "Movie deactivated" });
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        var result = await controller.ToggleStatus(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<OperationResult>(ok.Value);
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleStatus_CallsService_WithId()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.ToggleMovieStatusAsync(5, It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true });
        var controller = new MovieController(svc.Object, new Mock<IShowtimeService>().Object);

        await controller.ToggleStatus(5, CancellationToken.None);

        svc.Verify(s => s.ToggleMovieStatusAsync(5, CancellationToken.None), Times.Once);
    }

    // ── Showtime actions ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetShowtimes_ReturnsOkWithList()
    {
        var showtimeSvc = new Mock<IShowtimeService>();
        showtimeSvc.Setup(s => s.GetShowtimesByMovieAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync([MakeShowtimeResponse(), MakeShowtimeResponse()]);
        var controller = new MovieController(new Mock<IMovieService>().Object, showtimeSvc.Object);

        var result = await controller.GetShowtimes(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<ShowtimeResponse>>(ok.Value);
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSeatStatus_ReturnsOk()
    {
        var showtimeSvc = new Mock<IShowtimeService>();
        showtimeSvc.Setup(s => s.GetSeatStatusAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new SeatStatusResponse { ShowtimeId = 1, TotalSeats = 50, BookedSeats = [1, 2] });
        var controller = new MovieController(new Mock<IMovieService>().Object, showtimeSvc.Object);

        var result = await controller.GetSeatStatus(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<SeatStatusResponse>(ok.Value);
    }

    [Fact]
    public async Task CreateShowtime_Returns201()
    {
        var showtime = MakeShowtimeResponse(1);
        var showtimeSvc = new Mock<IShowtimeService>();
        showtimeSvc.Setup(s => s.CreateShowtimeAsync(It.IsAny<CreateShowtimeInput>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(showtime);
        var controller = new MovieController(new Mock<IMovieService>().Object, showtimeSvc.Object);

        var result = await controller.CreateShowtime(1, new CreateShowtimeInput(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task DeleteShowtime_Returns204()
    {
        var showtimeSvc = new Mock<IShowtimeService>();
        showtimeSvc.Setup(s => s.DeleteShowtimeAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new MovieController(new Mock<IMovieService>().Object, showtimeSvc.Object);

        var result = await controller.DeleteShowtime(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
