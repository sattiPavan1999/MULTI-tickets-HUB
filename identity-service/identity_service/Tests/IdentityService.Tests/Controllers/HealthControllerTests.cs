using IdentityService.Endpoints.Controllers;
using IdentityService.Core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Tests.Controllers;

public class HealthControllerTests
{
    private readonly ILogger<HealthController> _logger;

    public HealthControllerTests()
    {
        _logger = new LoggerFactory().CreateLogger<HealthController>();
    }

    [Fact]
    public void Live_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_Live")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = controller.Live();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void Live_ReturnsStatusAlive()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_LiveStatus")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = controller.Live() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var statusProperty = value.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("alive", statusProperty.GetValue(value));
    }

    [Fact]
    public void Live_ReturnsTimestamp()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_LiveTimestamp")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = controller.Live() as OkObjectResult;
        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var timestampProperty = value.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(value)!;
        Assert.True(timestamp >= beforeCall);
        Assert.True(timestamp <= afterCall);
    }

    [Fact]
    public async Task Ready_WithHealthyDatabase_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_Ready")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = await controller.Ready();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Ready_ReturnsStatusReady()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_ReadyStatus")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = await controller.Ready() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var statusProperty = value.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("ready", statusProperty.GetValue(value));
    }

    [Fact]
    public async Task Ready_ReturnsDatabaseStatus()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_ReadyDbStatus")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = await controller.Ready() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var databaseProperty = value.GetType().GetProperty("database");
        Assert.NotNull(databaseProperty);
        Assert.Equal("connected", databaseProperty.GetValue(value));
    }

    [Fact]
    public void V1Health_ReturnsOkResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_V1")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = controller.V1Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void V1Health_ReturnsStatusHealthy()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_V1Status")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = controller.V1Health() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var statusProperty = value.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("healthy", statusProperty.GetValue(value));
    }

    [Fact]
    public void V1Health_ReturnsServiceName()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_V1Service")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);

        // Act
        var result = controller.V1Health() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var serviceProperty = value.GetType().GetProperty("service");
        Assert.NotNull(serviceProperty);
        Assert.Equal("identity-service", serviceProperty.GetValue(value));
    }

    [Fact]
    public void V1Health_ReturnsTimestamp()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb_V1Timestamp")
            .Options;
        var context = new IdentityDbContext(options);
        var controller = new HealthController(context, _logger);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = controller.V1Health() as OkObjectResult;
        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var timestampProperty = value.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(value)!;
        Assert.True(timestamp >= beforeCall);
        Assert.True(timestamp <= afterCall);
    }
}
