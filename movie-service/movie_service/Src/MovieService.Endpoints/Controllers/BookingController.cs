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
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        input.UserId = userId;
        var booking = await bookingService.CreateBookingAsync(input, ct);
        return StatusCode(201, booking);
    }
}
