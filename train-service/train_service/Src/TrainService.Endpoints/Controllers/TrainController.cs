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
        [FromQuery] bool requiresAvailability = false)
    {
        var trains = await trainService.SearchTrainsAsync(source, destination, sortBy, requiresAvailability);
        return Ok(trains);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainResponse>> GetById(int id)
    {
        var train = await trainService.GetTrainByIdAsync(id);
        if (train is null) return NotFound();
        return Ok(train);
    }

    [HttpPost]
    public async Task<ActionResult<TrainResponse>> Create([FromBody] CreateTrainInput input)
    {
        var train = await trainService.CreateTrainAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = train.Id }, train);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TrainResponse>> Update(int id, [FromBody] UpdateTrainInput input)
    {
        var train = await trainService.UpdateTrainAsync(id, input);
        return Ok(train);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await trainService.DeleteTrainAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/seat-availability")]
    public async Task<ActionResult<List<SeatAvailabilityResponse>>> GetSeatAvailability(int id)
    {
        var seats = await trainService.GetSeatAvailabilityAsync(id);
        return Ok(seats);
    }

    [HttpPut("{id:int}/seat-availability")]
    public async Task<ActionResult<SeatAvailabilityResponse>> UpdateSeatAvailability(int id, [FromBody] SeatAvailabilityInput input)
    {
        var result = await trainService.UpdateSeatAvailabilityAsync(id, input);
        return Ok(result);
    }
}
