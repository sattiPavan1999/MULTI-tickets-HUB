namespace MovieService.Core.Models;

public class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public int Duration { get; set; }
    public required string PosterUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<Showtime> Showtimes { get; set; } = [];
}
