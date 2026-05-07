using MovieService.Core.DTOs;

namespace MovieService.Endpoints.GraphQL.Types;

public class ShowType
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int ScreenId { get; set; }
    public DateTime ShowTime { get; set; }
    public int AvailableSeats { get; set; }
    public ScreenType? Screen { get; set; }

    public static ShowType FromDto(ShowDto dto) => new()
    {
        Id = dto.Id,
        MovieId = dto.MovieId,
        ScreenId = dto.ScreenId,
        ShowTime = dto.ShowTime,
        AvailableSeats = dto.AvailableSeats,
        Screen = dto.Screen == null ? null : ScreenType.FromDto(dto.Screen)
    };
}
