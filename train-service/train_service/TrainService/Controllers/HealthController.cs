using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainService.Data;

namespace TrainService.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly TrainDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(TrainDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(new { status = "Ready", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new { status = "Not Ready", error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "Live", timestamp = DateTime.UtcNow });
    }
}
