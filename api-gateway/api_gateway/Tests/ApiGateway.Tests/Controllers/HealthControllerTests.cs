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

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        var controller = new HealthController(logger);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void Health_ShouldReturnOkResult()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result = controller.Health();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Health_ShouldReturnHealthyStatus()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result = controller.Health() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var statusProperty = value.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("Healthy", statusProperty.GetValue(value));
    }

    [Fact]
    public void Health_ShouldReturnTimestamp()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = controller.Health() as OkObjectResult;
        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var timestampProperty = value.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(value)!;

        Assert.True(timestamp >= beforeCall && timestamp <= afterCall);
    }

    [Fact]
    public void Ready_ShouldReturnOkResult()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result = controller.Ready();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Ready_ShouldReturnReadyStatus()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result = controller.Ready() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var statusProperty = value.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("Ready", statusProperty.GetValue(value));
    }

    [Fact]
    public void Ready_ShouldReturnTimestamp()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = controller.Ready() as OkObjectResult;
        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var timestampProperty = value.GetType().GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        var timestamp = (DateTime)timestampProperty.GetValue(value)!;

        Assert.True(timestamp >= beforeCall && timestamp <= afterCall);
    }

    [Fact]
    public void Live_ShouldReturnOkResult()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result = controller.Live();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Live_ShouldReturnLiveStatus()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result = controller.Live() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        var value = result.Value;
        Assert.NotNull(value);

        var statusProperty = value.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("Live", statusProperty.GetValue(value));
    }

    [Fact]
    public void Live_ShouldReturnTimestamp()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);
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

        Assert.True(timestamp >= beforeCall && timestamp <= afterCall);
    }

    [Fact]
    public void MultipleHealthCalls_ShouldReturnConsistentResults()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result1 = controller.Health() as OkObjectResult;
        var result2 = controller.Health() as OkObjectResult;

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        var value1 = result1.Value;
        var value2 = result2.Value;
        Assert.NotNull(value1);
        Assert.NotNull(value2);

        var status1 = value1.GetType().GetProperty("status")?.GetValue(value1);
        var status2 = value2.GetType().GetProperty("status")?.GetValue(value2);

        Assert.Equal(status1, status2);
        Assert.Equal("Healthy", status1);
    }

    [Fact]
    public void MultipleReadyCalls_ShouldReturnConsistentResults()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result1 = controller.Ready() as OkObjectResult;
        var result2 = controller.Ready() as OkObjectResult;

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        var value1 = result1.Value;
        var value2 = result2.Value;
        Assert.NotNull(value1);
        Assert.NotNull(value2);

        var status1 = value1.GetType().GetProperty("status")?.GetValue(value1);
        var status2 = value2.GetType().GetProperty("status")?.GetValue(value2);

        Assert.Equal(status1, status2);
        Assert.Equal("Ready", status1);
    }

    [Fact]
    public void MultipleLiveCalls_ShouldReturnConsistentResults()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var result1 = controller.Live() as OkObjectResult;
        var result2 = controller.Live() as OkObjectResult;

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        var value1 = result1.Value;
        var value2 = result2.Value;
        Assert.NotNull(value1);
        Assert.NotNull(value2);

        var status1 = value1.GetType().GetProperty("status")?.GetValue(value1);
        var status2 = value2.GetType().GetProperty("status")?.GetValue(value2);

        Assert.Equal(status1, status2);
        Assert.Equal("Live", status1);
    }

    [Fact]
    public void AllEndpoints_ShouldReturnDifferentStatuses()
    {
        // Arrange
        var logger = new TestLogger();
        var controller = new HealthController(logger);

        // Act
        var healthResult = controller.Health() as OkObjectResult;
        var readyResult = controller.Ready() as OkObjectResult;
        var liveResult = controller.Live() as OkObjectResult;

        // Assert
        Assert.NotNull(healthResult);
        Assert.NotNull(readyResult);
        Assert.NotNull(liveResult);

        var healthStatus = healthResult.Value?.GetType().GetProperty("status")?.GetValue(healthResult.Value);
        var readyStatus = readyResult.Value?.GetType().GetProperty("status")?.GetValue(readyResult.Value);
        var liveStatus = liveResult.Value?.GetType().GetProperty("status")?.GetValue(liveResult.Value);

        Assert.Equal("Healthy", healthStatus);
        Assert.Equal("Ready", readyStatus);
        Assert.Equal("Live", liveStatus);
    }
}
