namespace MovieService.Core.DTOs;

public class CreateMovieInput
{
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
}
