using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainService.Core.Data;

namespace TrainService.Endpoints.Controllers;

[ApiController]
public class HealthController(TrainDbContext dbContext) : ControllerBase
{
    [HttpGet("health/live")]
    public IActionResult Live() => Ok(new { status = "alive", timestamp = DateTime.UtcNow });

    [HttpGet("health/ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
            return Ok(new { status = "ready", database = "connected" });
        }
        catch
        {
            return StatusCode(503, new { status = "unavailable", database = "disconnected" });
        }
    }
}
