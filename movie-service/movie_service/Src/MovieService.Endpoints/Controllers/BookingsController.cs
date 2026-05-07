using Microsoft.AspNetCore.Mvc;
using MovieService.Core.DTOs;
using MovieService.Endpoints.GraphQL.Inputs;
using MovieService.Core.Services;

namespace MovieService.Endpoints.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create([FromBody] BookSeatsInput input)
    {
        var booking = await _bookingService.BookSeatsAsync(input.UserId, input.ShowId, input.SelectedSeatIds);
        return CreatedAtAction(null, new { id = booking.Id }, booking);
    }

    [HttpPut("{bookingId:int}/cancel")]
    public async Task<ActionResult<BookingDto>> Cancel(int bookingId, [FromQuery] int userId)
    {
        var booking = await _bookingService.CancelBookingAsync(bookingId, userId);
        return Ok(booking);
    }
}
