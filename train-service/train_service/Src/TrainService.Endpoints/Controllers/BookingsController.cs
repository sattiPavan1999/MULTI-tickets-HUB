using Microsoft.AspNetCore.Mvc;
using TrainService.Core.DTOs;
using TrainService.Core.Services;

namespace TrainService.Endpoints.Controllers;

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
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingInput input)
    {
        var booking = await _bookingService.CreateBookingAsync(input);
        return CreatedAtAction(null, new { id = booking.Id }, booking);
    }

    [HttpPut("{bookingId:int}/cancel")]
    public async Task<ActionResult<CancelBookingResponse>> Cancel(int bookingId, [FromQuery] int userId)
    {
        var response = await _bookingService.CancelBookingAsync(bookingId, userId);
        return Ok(response);
    }
}
