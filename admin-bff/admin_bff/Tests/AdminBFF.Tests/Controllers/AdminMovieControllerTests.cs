using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using AdminBFF.Endpoints.Controllers;

namespace AdminBFF.Tests.Controllers;

public class AdminMovieControllerTests
{
    private static readonly Faker Fake = new();

    private static MovieDto MakeMovie(int id = 0) => new()
    {
        Id = id > 0 ? id : Fake.Random.Int(1, 1000),
        Title = Fake.Lorem.Word(),
        Genre = "Action",
        Duration = 120,
        PosterUrl = "https://example.com/p.jpg",
        IsActive = true
    };

    [Fact]
    public async Task Create_Returns201()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.CreateMovieAsync(It.IsAny<CreateMovieRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeMovie(1));
        var controller = new AdminMovieController(svc.Object);

        var result = await controller.Create(new CreateMovieRequest { Title = "T", Genre = "G", Duration = 100, PosterUrl = "https://e.com/p.jpg" }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Update_Returns200()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.UpdateMovieAsync(1, It.IsAny<UpdateMovieRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeMovie(1));
        var controller = new AdminMovieController(svc.Object);

        var result = await controller.Update(1, new UpdateMovieRequest { Title = "New" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.DeleteMovieAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var controller = new AdminMovieController(svc.Object);

        var result = await controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ToggleStatus_Returns200WithOperationResult()
    {
        var svc = new Mock<IMovieService>();
        svc.Setup(s => s.ToggleMovieStatusAsync(1, It.IsAny<CancellationToken>()))
           .ReturnsAsync(new OperationResult { Success = true, Message = "Movie deactivated" });
        var controller = new AdminMovieController(svc.Object);

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
        var controller = new AdminMovieController(svc.Object);

        await controller.ToggleStatus(5, CancellationToken.None);

        svc.Verify(s => s.ToggleMovieStatusAsync(5, CancellationToken.None), Times.Once);
    }
}
