using IdentityService.Endpoints.Controllers;
using IdentityService.Core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Tests.Controllers;

public class HealthControllerTests
{
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthTestDb")
            .Options;
        var context = new IdentityDbContext(options);
        var logger = new LoggerFactory().CreateLogger<HealthController>();
        _controller = new HealthController(context, logger);
    }

    [Fact]
    public void Live_ReturnsOkResult()
    {
        var result = _controller.Live();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void Live_ReturnsStatusAlive()
    {
        var result = _controller.Live() as OkObjectResult;

        Assert.NotNull(result);
        var statusProperty = result.Value?.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("alive", statusProperty.GetValue(result.Value));
    }

    [Fact]
    public void Live_ReturnsTimestamp()
    {
        var before = DateTime.UtcNow;
        var result = _controller.Live() as OkObjectResult;
        var after = DateTime.UtcNow;

        Assert.NotNull(result);
        var timestampProperty = result.Value?.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(result.Value)!;
        Assert.True(timestamp >= before && timestamp <= after);
    }

    [Fact]
    public async Task Ready_WithHealthyDatabase_ReturnsOkResult()
    {
        var result = await _controller.Ready();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Ready_ReturnsStatusReady()
    {
        var result = await _controller.Ready() as OkObjectResult;

        Assert.NotNull(result);
        var statusProperty = result.Value?.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("ready", statusProperty.GetValue(result.Value));
    }

    [Fact]
    public async Task Ready_ReturnsDatabaseStatus()
    {
        var result = await _controller.Ready() as OkObjectResult;

        Assert.NotNull(result);
        var databaseProperty = result.Value?.GetType().GetProperty("database");
        Assert.NotNull(databaseProperty);
        Assert.Equal("connected", databaseProperty.GetValue(result.Value));
    }

    [Fact]
    public void V1Health_ReturnsOkResult()
    {
        var result = _controller.V1Health();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void V1Health_ReturnsStatusHealthy()
    {
        var result = _controller.V1Health() as OkObjectResult;

        Assert.NotNull(result);
        var statusProperty = result.Value?.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("healthy", statusProperty.GetValue(result.Value));
    }

    [Fact]
    public void V1Health_ReturnsServiceName()
    {
        var result = _controller.V1Health() as OkObjectResult;

        Assert.NotNull(result);
        var serviceProperty = result.Value?.GetType().GetProperty("service");
        Assert.NotNull(serviceProperty);
        Assert.Equal("identity-service", serviceProperty.GetValue(result.Value));
    }

    [Fact]
    public void V1Health_ReturnsTimestamp()
    {
        var before = DateTime.UtcNow;
        var result = _controller.V1Health() as OkObjectResult;
        var after = DateTime.UtcNow;

        Assert.NotNull(result);
        var timestampProperty = result.Value?.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(result.Value)!;
        Assert.True(timestamp >= before && timestamp <= after);
    }
}
