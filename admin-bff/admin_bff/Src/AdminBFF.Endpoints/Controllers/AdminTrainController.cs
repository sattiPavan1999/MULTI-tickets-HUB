using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Endpoints.Controllers;

[ApiController]
[Route("api/admin/trains")]
[Authorize(Roles = "Admin")]
public class AdminTrainController(ITrainService trainService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TrainDto>> Create([FromBody] CreateTrainRequest request, CancellationToken ct)
    {
        var train = await trainService.CreateTrainAsync(request, ct);
        return CreatedAtAction(null, new { id = train.Id }, train);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TrainDto>> Update(int id, [FromBody] UpdateTrainRequest request, CancellationToken ct)
    {
        var train = await trainService.UpdateTrainAsync(id, request, ct);
        return Ok(train);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await trainService.DeleteTrainAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/seat-availability")]
    public async Task<ActionResult<List<SeatAvailabilityDto>>> GetSeatAvailability(int id, CancellationToken ct)
    {
        var seats = await trainService.GetSeatAvailabilityAsync(id, ct);
        return Ok(seats);
    }

    [HttpPut("{id:int}/seat-availability")]
    public async Task<ActionResult<SeatAvailabilityDto>> UpdateSeatAvailability(int id, [FromBody] UpdateSeatAvailabilityRequest request, CancellationToken ct)
    {
        var result = await trainService.UpdateSeatAvailabilityAsync(id, request, ct);
        return Ok(result);
    }
}
