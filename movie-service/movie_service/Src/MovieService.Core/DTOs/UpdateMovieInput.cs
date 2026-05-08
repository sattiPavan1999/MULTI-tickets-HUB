namespace MovieService.Core.DTOs;

public class UpdateMovieInput
{
    public string? Title { get; set; }
    public string? Genre { get; set; }
    public int? Duration { get; set; }
    public string? PosterUrl { get; set; }
}
