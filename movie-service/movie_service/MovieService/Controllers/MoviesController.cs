using Microsoft.AspNetCore.Mvc;

namespace MovieService.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    public class AddMovieRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }

    [HttpPost]
    public ActionResult<object> Add([FromBody] AddMovieRequest input)
    {
        return Accepted(new { acknowledged = true, title = input.Title });
    }
}
