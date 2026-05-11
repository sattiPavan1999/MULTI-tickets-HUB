using Microsoft.AspNetCore.Mvc;
using TrainService.Core.DTOs;
using TrainService.Core.Services;

namespace TrainService.Endpoints.Controllers;

[ApiController]
[Route("api/trains")]
public class TrainController(ITrainService trainService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TrainResponse>>> GetAll(
        [FromQuery] string? source,
        [FromQuery] string? destination,
        [FromQuery] string? sortBy,
        [FromQuery] bool requiresAvailability = false,
        CancellationToken ct = default)
    {
        var trains = await trainService.SearchTrainsAsync(source, destination, sortBy, requiresAvailability, ct);
        return Ok(trains);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainResponse>> GetById(int id, CancellationToken ct)
    {
        var train = await trainService.GetTrainByIdAsync(id, ct);
        if (train is null) return NotFound();
        return Ok(train);
    }

    [HttpPost]
    public async Task<ActionResult<TrainResponse>> Create([FromBody] CreateTrainInput input, CancellationToken ct)
    {
        var train = await trainService.CreateTrainAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = train.Id }, train);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TrainResponse>> Update(int id, [FromBody] UpdateTrainInput input, CancellationToken ct)
    {
        var train = await trainService.UpdateTrainAsync(id, input, ct);
        return Ok(train);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await trainService.DeleteTrainAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/seat-availability")]
    public async Task<ActionResult<List<SeatAvailabilityResponse>>> GetSeatAvailability(int id, CancellationToken ct)
    {
        var seats = await trainService.GetSeatAvailabilityAsync(id, ct);
        return Ok(seats);
    }

    [HttpPut("{id:int}/seat-availability")]
    public async Task<ActionResult<SeatAvailabilityResponse>> UpdateSeatAvailability(int id, [FromBody] SeatAvailabilityInput input, CancellationToken ct)
    {
        var result = await trainService.UpdateSeatAvailabilityAsync(id, input, ct);
        return Ok(result);
    }
}
