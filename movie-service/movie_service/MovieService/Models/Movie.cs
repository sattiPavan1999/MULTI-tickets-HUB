namespace MovieService.Models;

public class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Language { get; set; }
    public required string Format { get; set; }
    public int DurationMinutes { get; set; }
    public required string Synopsis { get; set; }
    public string? PosterUrl { get; set; }

    // Navigation properties
    public ICollection<Show> Shows { get; set; } = new List<Show>();
}
