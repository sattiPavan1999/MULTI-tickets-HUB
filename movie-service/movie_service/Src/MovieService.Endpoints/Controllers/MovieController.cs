using Microsoft.AspNetCore.Mvc;
using MovieService.Core.DTOs;
using MovieService.Core.Services;

namespace MovieService.Endpoints.Controllers;

[ApiController]
[Route("api/movies")]
public class MovieController(IMovieService movieService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MovieResponse>>> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var movies = await movieService.GetAllMoviesAsync(ct);
        if (activeOnly is true)
            movies = movies.Where(m => m.IsActive).ToList();
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
    public async Task<ActionResult<MovieResponse>> Create([FromBody] CreateMovieInput input, CancellationToken ct)
    {
        var movie = await movieService.CreateMovieAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieResponse>> Update(int id, [FromBody] UpdateMovieInput input, CancellationToken ct)
    {
        var movie = await movieService.UpdateMovieAsync(id, input, ct);
        return Ok(movie);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await movieService.DeleteMovieAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/toggle-status")]
    public async Task<ActionResult<OperationResult>> ToggleStatus(int id, CancellationToken ct)
    {
        var result = await movieService.ToggleMovieStatusAsync(id, ct);
        return Ok(result);
    }
}
