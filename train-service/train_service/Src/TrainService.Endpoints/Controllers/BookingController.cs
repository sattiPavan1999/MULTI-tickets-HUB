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
        var booking = await bookingService.CreateBookingAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok();
}
