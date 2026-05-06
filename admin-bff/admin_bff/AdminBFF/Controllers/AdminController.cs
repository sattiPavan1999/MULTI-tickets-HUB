using System.Security.Claims;
using AdminBFF.DTOs;
using AdminBFF.Models;
using AdminBFF.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public class CancelBookingRequest
    {
        public int BookingId { get; set; }
        public string BookingType { get; set; } = string.Empty;
    }

    [HttpPut("users/{userId:int}/deactivate")]
    public async Task<ActionResult<OperationResultDto>> DeactivateUser(int userId)
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        return Ok(await _adminService.DeactivateUserAsync(userId, currentUserId));
    }

    [HttpPost("bookings/cancel")]
    public async Task<ActionResult<OperationResultDto>> CancelBooking([FromBody] CancelBookingRequest request)
    {
        return Ok(await _adminService.CancelBookingAsync(request.BookingId, request.BookingType));
    }

    [HttpPost("trains")]
    public async Task<ActionResult<TrainDto>> AddTrain([FromBody] AddTrainInput input)
    {
        var train = await _adminService.AddTrainAsync(input);
        return CreatedAtAction(null, train);
    }

    [HttpPost("movies")]
    public async Task<ActionResult<MovieDto>> AddMovie([FromBody] AddMovieInput input)
    {
        var movie = await _adminService.AddMovieAsync(input);
        return CreatedAtAction(null, movie);
    }
}
