namespace AdminBFF.DTOs;

public record MovieDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Genre { get; init; }
    public required string Language { get; init; }
    public required string Format { get; init; } // "2D", "3D", or "IMAX"
    public int DurationMinutes { get; init; }
    public required string Synopsis { get; init; }
    public required string PosterUrl { get; init; }
}
