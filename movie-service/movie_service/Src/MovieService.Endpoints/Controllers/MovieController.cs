using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.Core.DTOs;
using MovieService.Core.Services;

namespace MovieService.Endpoints.Controllers;

[ApiController]
[Route("api/movies")]
public class MovieController(IMovieService movieService, IShowtimeService showtimeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MovieResponse>>> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var movies = await movieService.GetAllMoviesAsync(activeOnly, ct);
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieResponse>> GetById(int id, CancellationToken ct)
    {
        var movie = await movieService.GetMovieByIdAsync(id, ct);
        if (movie is null) return NotFound();
        return Ok(movie);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MovieResponse>> Create([FromBody] CreateMovieInput input, CancellationToken ct)
    {
        var movie = await movieService.CreateMovieAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MovieResponse>> Update(int id, [FromBody] UpdateMovieInput input, CancellationToken ct)
    {
        var movie = await movieService.UpdateMovieAsync(id, input, ct);
        return Ok(movie);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await movieService.DeleteMovieAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OperationResult>> ToggleStatus(int id, CancellationToken ct)
    {
        var result = await movieService.ToggleMovieStatusAsync(id, ct);
        return Ok(result);
    }

    // ── Showtime endpoints ────────────────────────────────────────────────────

    [HttpGet("{movieId:int}/showtimes")]
    public async Task<ActionResult<List<ShowtimeResponse>>> GetShowtimes(int movieId, CancellationToken ct)
    {
        var showtimes = await showtimeService.GetShowtimesByMovieAsync(movieId, ct);
        return Ok(showtimes);
    }

    [HttpPost("{movieId:int}/showtimes")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ShowtimeResponse>> CreateShowtime(int movieId, [FromBody] CreateShowtimeInput input, CancellationToken ct)
    {
        input.MovieId = movieId;
        var showtime = await showtimeService.CreateShowtimeAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, showtime);
    }

    [HttpGet("showtimes/{id:int}/seats")]
    public async Task<ActionResult<SeatStatusResponse>> GetSeatStatus(int id, CancellationToken ct)
    {
        var status = await showtimeService.GetSeatStatusAsync(id, ct);
        return Ok(status);
    }

    [HttpDelete("showtimes/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteShowtime(int id, CancellationToken ct)
    {
        await showtimeService.DeleteShowtimeAsync(id, ct);
        return NoContent();
    }
}
