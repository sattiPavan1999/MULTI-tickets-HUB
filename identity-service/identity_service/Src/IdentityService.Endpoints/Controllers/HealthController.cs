using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Endpoints.Controllers;

/// <summary>
/// Health check controller
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IdentityDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Liveness probe - checks if the service is running
    /// </summary>
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe - checks if the service is ready to accept requests
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(new { status = "ready", timestamp = DateTime.UtcNow, database = "connected" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { status = "not ready", timestamp = DateTime.UtcNow, database = "disconnected" });
        }
    }

    /// <summary>
    /// v1 health endpoint
    /// </summary>
    [HttpGet("/v1/identity/health")]
    public IActionResult V1Health()
    {
        return Ok(new { status = "healthy", service = "identity-service", timestamp = DateTime.UtcNow });
    }
}
