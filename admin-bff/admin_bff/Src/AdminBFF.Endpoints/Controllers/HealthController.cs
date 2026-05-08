using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Endpoints.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("health/live")]
    public IActionResult Live() => Ok(new { status = "alive", timestamp = DateTime.UtcNow });

    [HttpGet("health/ready")]
    public IActionResult Ready() => Ok(new { status = "ready", service = "admin-bff" });
}
