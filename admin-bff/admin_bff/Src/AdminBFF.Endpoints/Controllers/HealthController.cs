using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Endpoints.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Liveness()
    {
        return Ok(new { status = "UP", timestamp = DateTime.UtcNow });
    }

    [HttpGet("ready")]
    public IActionResult Readiness()
    {
        return Ok(new { status = "READY", timestamp = DateTime.UtcNow });
    }
}

[ApiController]
[Route("v1/admin-bff")]
public class AdminBffHealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { service = "admin-bff", status = "healthy", timestamp = DateTime.UtcNow });
    }
}
