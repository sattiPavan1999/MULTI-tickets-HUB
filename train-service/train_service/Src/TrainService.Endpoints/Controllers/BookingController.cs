using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainService.Core.DTOs;
using TrainService.Core.Services;

namespace TrainService.Endpoints.Controllers;

[ApiController]
[Route("api/trains/bookings")]
[Authorize]
public class BookingController(ITrainBookingService bookingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TrainBookingResponse>> Create([FromBody] CreateTrainBookingInput input, CancellationToken ct)
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        input.UserId = userId;
        var booking = await bookingService.CreateBookingAsync(input, ct);
        return StatusCode(201, booking);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<OperationResult>> Cancel(int id, CancellationToken ct)
    {
        var result = await bookingService.CancelBookingAsync(id, ct);
        return Ok(result);
    }
}
