using Microsoft.AspNetCore.Mvc;
using TrainService.Core.DTOs;
using TrainService.Core.Services;

namespace TrainService.Endpoints.Controllers;

[ApiController]
[Route("api/trains/bookings")]
public class BookingController(ITrainBookingService bookingService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TrainBookingResponse>> Create([FromBody] CreateTrainBookingInput input)
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        input.UserId = userId;
        var booking = await bookingService.CreateBookingAsync(input);
        return StatusCode(201, booking);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<TrainBookingResponse>>> GetMyBookings()
    {
        if (!int.TryParse(Request.Headers["X-User-Id"].FirstOrDefault(), out var userId) || userId <= 0)
            return Unauthorized();

        var bookings = await bookingService.GetMyBookingsAsync(userId);
        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainBookingResponse>> GetById(int id)
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
