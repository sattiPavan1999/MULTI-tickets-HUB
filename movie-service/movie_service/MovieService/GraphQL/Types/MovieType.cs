using MovieService.DTOs;

namespace MovieService.GraphQL.Types;

public class MovieType
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required string Language { get; set; }
    public required string Format { get; set; }
    public int DurationMinutes { get; set; }
    public required string Synopsis { get; set; }
    public string? PosterUrl { get; set; }

    public static MovieType FromDto(MovieDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        Genre = dto.Genre,
        Language = dto.Language,
        Format = dto.Format,
        DurationMinutes = dto.DurationMinutes,
        Synopsis = dto.Synopsis,
        PosterUrl = dto.PosterUrl
    };
}
