using ApiGateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Tests.Controllers;

public class HealthControllerTests
{
    private class TestLogger : ILogger<HealthController>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    private readonly HealthController _controller = new(new TestLogger());

    [Fact]
    public void Health_ShouldReturnOkResult()
    {
        Assert.IsType<OkObjectResult>(_controller.Health());
    }

    [Fact]
    public void Health_ShouldReturnHealthyStatus()
    {
        var result = _controller.Health() as OkObjectResult;

        Assert.NotNull(result);
        var statusProperty = result.Value?.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("Healthy", statusProperty.GetValue(result.Value));
    }

    [Fact]
    public void Health_ShouldReturnTimestamp()
    {
        var before = DateTime.UtcNow;
        var result = _controller.Health() as OkObjectResult;
        var after = DateTime.UtcNow;

        Assert.NotNull(result);
        var timestampProperty = result.Value?.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(result.Value)!;
        Assert.True(timestamp >= before && timestamp <= after);
    }

    [Fact]
    public void Ready_ShouldReturnOkResult()
    {
        Assert.IsType<OkObjectResult>(_controller.Ready());
    }

    [Fact]
    public void Ready_ShouldReturnReadyStatus()
    {
        var result = _controller.Ready() as OkObjectResult;

        Assert.NotNull(result);
        var statusProperty = result.Value?.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("Ready", statusProperty.GetValue(result.Value));
    }

    [Fact]
    public void Ready_ShouldReturnTimestamp()
    {
        var before = DateTime.UtcNow;
        var result = _controller.Ready() as OkObjectResult;
        var after = DateTime.UtcNow;

        Assert.NotNull(result);
        var timestampProperty = result.Value?.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(result.Value)!;
        Assert.True(timestamp >= before && timestamp <= after);
    }

    [Fact]
    public void Live_ShouldReturnOkResult()
    {
        Assert.IsType<OkObjectResult>(_controller.Live());
    }

    [Fact]
    public void Live_ShouldReturnLiveStatus()
    {
        var result = _controller.Live() as OkObjectResult;

        Assert.NotNull(result);
        var statusProperty = result.Value?.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("Live", statusProperty.GetValue(result.Value));
    }

    [Fact]
    public void Live_ShouldReturnTimestamp()
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
    public void AllEndpoints_ShouldReturnDifferentStatuses()
    {
        var healthStatus = (_controller.Health() as OkObjectResult)?.Value?.GetType().GetProperty("status")?.GetValue((_controller.Health() as OkObjectResult)!.Value);
        var readyStatus = (_controller.Ready() as OkObjectResult)?.Value?.GetType().GetProperty("status")?.GetValue((_controller.Ready() as OkObjectResult)!.Value);
        var liveStatus = (_controller.Live() as OkObjectResult)?.Value?.GetType().GetProperty("status")?.GetValue((_controller.Live() as OkObjectResult)!.Value);

        Assert.Equal("Healthy", healthStatus);
        Assert.Equal("Ready", readyStatus);
        Assert.Equal("Live", liveStatus);
    }
}
