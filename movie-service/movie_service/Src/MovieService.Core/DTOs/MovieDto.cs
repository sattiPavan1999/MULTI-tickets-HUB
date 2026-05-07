namespace MovieService.Core.DTOs;

public class MovieDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Language { get; set; }
    public required string Format { get; set; }
    public int DurationMinutes { get; set; }
    public required string Synopsis { get; set; }
    public string? PosterUrl { get; set; }
}
