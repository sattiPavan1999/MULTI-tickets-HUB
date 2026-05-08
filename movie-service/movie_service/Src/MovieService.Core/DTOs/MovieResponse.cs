namespace MovieService.Core.DTOs;

public class MovieResponse
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public int Duration { get; set; }
    public required string PosterUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
