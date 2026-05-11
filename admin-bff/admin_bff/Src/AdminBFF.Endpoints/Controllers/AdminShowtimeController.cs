using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Endpoints.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
public class AdminShowtimeController(IMovieService movieService) : ControllerBase
{
    [HttpGet("api/admin/movies/{movieId:int}/showtimes")]
    public async Task<ActionResult<List<ShowtimeDto>>> GetShowtimes(int movieId)
    {
        var showtimes = await movieService.GetShowtimesAsync(movieId);
        return Ok(showtimes);
    }

    [HttpPost("api/admin/movies/{movieId:int}/showtimes")]
    public async Task<ActionResult<ShowtimeDto>> CreateShowtime(int movieId, [FromBody] CreateShowtimeRequest request)
    {
        request.MovieId = movieId;
        var showtime = await movieService.CreateShowtimeAsync(request);
        return CreatedAtAction(null, new { id = showtime.Id }, showtime);
    }

    [HttpDelete("api/admin/movies/showtimes/{id:int}")]
    public async Task<IActionResult> DeleteShowtime(int id)
    {
        await movieService.DeleteShowtimeAsync(id);
        return NoContent();
    }
}
