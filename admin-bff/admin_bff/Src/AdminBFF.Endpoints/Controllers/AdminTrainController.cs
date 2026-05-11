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
    public async Task<ActionResult<TrainDto>> Create([FromBody] CreateTrainRequest request)
    {
        var train = await trainService.CreateTrainAsync(request);
        return CreatedAtAction(null, new { id = train.Id }, train);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TrainDto>> Update(int id, [FromBody] UpdateTrainRequest request)
    {
        var train = await trainService.UpdateTrainAsync(id, request);
        return Ok(train);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await trainService.DeleteTrainAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/seat-availability")]
    public async Task<ActionResult<List<SeatAvailabilityDto>>> GetSeatAvailability(int id)
    {
        var seats = await trainService.GetSeatAvailabilityAsync(id);
        return Ok(seats);
    }

    [HttpPut("{id:int}/seat-availability")]
    public async Task<ActionResult<SeatAvailabilityDto>> UpdateSeatAvailability(int id, [FromBody] UpdateSeatAvailabilityRequest request)
    {
        var result = await trainService.UpdateSeatAvailabilityAsync(id, request);
        return Ok(result);
    }
}
