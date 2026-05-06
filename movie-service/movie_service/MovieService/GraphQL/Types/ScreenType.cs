using MovieService.DTOs;

namespace MovieService.GraphQL.Types;

public class ScreenType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int TotalSeats { get; set; }
    public CinemaType? Cinema { get; set; }

    public static ScreenType FromDto(ScreenDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        TotalSeats = dto.TotalSeats,
        Cinema = dto.Cinema == null ? null : CinemaType.FromDto(dto.Cinema)
    };
}
