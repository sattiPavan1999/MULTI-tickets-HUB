using Microsoft.AspNetCore.Mvc;
using MovieService.Core.DTOs;
using MovieService.Core.Services;

namespace MovieService.Endpoints.Controllers;

[ApiController]
[Route("api/movies/bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingInput input)
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        input.UserId = userId;
        var booking = await bookingService.CreateBookingAsync(input);
        return StatusCode(201, booking);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<BookingResponse>>> GetMyBookings()
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        var bookings = await bookingService.GetMyBookingsAsync(userId);
        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id)
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        var booking = await bookingService.GetBookingByIdAsync(id, userId);
        return Ok(booking);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<OperationResult>> Cancel(int id)
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        var result = await bookingService.CancelBookingAsync(id, userId);
        return Ok(result);
    }
}
