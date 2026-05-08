using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminBFF.Endpoints.Controllers;

[ApiController]
[Route("api/admin/movies")]
[Authorize(Roles = "Admin")]
public class AdminMovieController(IMovieService movieService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MovieDto>> Create([FromBody] CreateMovieRequest request, CancellationToken ct)
    {
        var movie = await movieService.CreateMovieAsync(request, ct);
        return CreatedAtAction(null, new { id = movie.Id }, movie);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieDto>> Update(int id, [FromBody] UpdateMovieRequest request, CancellationToken ct)
    {
        var movie = await movieService.UpdateMovieAsync(id, request, ct);
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
