using AdminBFF.Endpoints.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Liveness_Should_Return_Ok_With_Status()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.Liveness();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var value = okResult.Value as dynamic;
        Assert.NotNull(value);
    }

    [Fact]
    public void Readiness_Should_Return_Ok_With_Status()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.Readiness();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void Liveness_Should_Return_200_StatusCode()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.Liveness() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void Readiness_Should_Return_200_StatusCode()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.Readiness() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }
}

public class AdminBffHealthControllerTests
{
    [Fact]
    public void Health_Should_Return_Ok_With_Service_Status()
    {
        // Arrange
        var controller = new AdminBffHealthController();

        // Act
        var result = controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void Health_Should_Return_200_StatusCode()
    {
        // Arrange
        var controller = new AdminBffHealthController();

        // Act
        var result = controller.Health() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }
}
