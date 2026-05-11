using Microsoft.AspNetCore.Mvc;
using MovieService.Core.DTOs;
using MovieService.Core.Services;

namespace MovieService.Endpoints.Controllers;

[ApiController]
[Route("api/movies")]
public class MovieController(IMovieService movieService, IShowtimeService showtimeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MovieResponse>>> GetAll([FromQuery] bool? activeOnly)
    {
        var movies = await movieService.GetAllMoviesAsync(activeOnly);
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieResponse>> GetById(int id)
    {
        var movie = await movieService.GetMovieByIdAsync(id);
        if (movie is null) return NotFound();
        return Ok(movie);
    }

    [HttpPost]
    public async Task<ActionResult<MovieResponse>> Create([FromBody] CreateMovieInput input)
    {
        var movie = await movieService.CreateMovieAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieResponse>> Update(int id, [FromBody] UpdateMovieInput input)
    {
        var movie = await movieService.UpdateMovieAsync(id, input);
        return Ok(movie);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await movieService.DeleteMovieAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/toggle-status")]
    public async Task<ActionResult<OperationResult>> ToggleStatus(int id)
    {
        var result = await movieService.ToggleMovieStatusAsync(id);
        return Ok(result);
    }

    // ── Showtime endpoints ────────────────────────────────────────────────────

    [HttpGet("{movieId:int}/showtimes")]
    public async Task<ActionResult<List<ShowtimeResponse>>> GetShowtimes(int movieId)
    {
        var showtimes = await showtimeService.GetShowtimesByMovieAsync(movieId);
        return Ok(showtimes);
    }

    [HttpPost("{movieId:int}/showtimes")]
    public async Task<ActionResult<ShowtimeResponse>> CreateShowtime(int movieId, [FromBody] CreateShowtimeInput input)
    {
        input.MovieId = movieId;
        var showtime = await showtimeService.CreateShowtimeAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, showtime);
    }

    [HttpGet("showtimes/{id:int}/seats")]
    public async Task<ActionResult<SeatStatusResponse>> GetSeatStatus(int id)
    {
        var status = await showtimeService.GetSeatStatusAsync(id);
        return Ok(status);
    }

    [HttpDelete("showtimes/{id:int}")]
    public async Task<IActionResult> DeleteShowtime(int id)
    {
        await showtimeService.DeleteShowtimeAsync(id);
        return NoContent();
    }
}
