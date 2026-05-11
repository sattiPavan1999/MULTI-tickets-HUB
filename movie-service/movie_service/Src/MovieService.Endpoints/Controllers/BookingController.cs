using Microsoft.AspNetCore.Mvc;
using MovieService.Core.DTOs;
using MovieService.Core.Services;

namespace MovieService.Endpoints.Controllers;

[ApiController]
[Route("api/movies/bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingInput input, CancellationToken ct)
    {
        var booking = await bookingService.CreateBookingAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok();
}
