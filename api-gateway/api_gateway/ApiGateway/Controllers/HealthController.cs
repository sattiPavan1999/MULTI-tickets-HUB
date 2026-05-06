using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("/health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check endpoint called");
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }

    [HttpGet]
    [Route("/health/ready")]
    public IActionResult Ready()
    {
        _logger.LogInformation("Readiness check endpoint called");
        return Ok(new { status = "Ready", timestamp = DateTime.UtcNow });
    }

    [HttpGet]
    [Route("/health/live")]
    public IActionResult Live()
    {
        _logger.LogInformation("Liveness check endpoint called");
        return Ok(new { status = "Live", timestamp = DateTime.UtcNow });
    }
}
