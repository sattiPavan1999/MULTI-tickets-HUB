using Microsoft.AspNetCore.Mvc;
using TrainService.Core.DTOs;
using TrainService.Core.Services;

namespace TrainService.Endpoints.Controllers;

[ApiController]
[Route("api/trains/bookings")]
public class BookingController(ITrainBookingService bookingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TrainBookingResponse>> Create([FromBody] CreateTrainBookingInput input, CancellationToken ct)
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        input.UserId = userId;
        var booking = await bookingService.CreateBookingAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<OperationResult>> Cancel(int id, CancellationToken ct)
    {
        var result = await bookingService.CancelBookingAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok();
}
