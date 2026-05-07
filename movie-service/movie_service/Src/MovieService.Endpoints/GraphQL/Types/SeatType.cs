using MovieService.Core.DTOs;

namespace MovieService.Endpoints.GraphQL.Types;

public class SeatType
{
    public int Id { get; set; }
    public int ScreenId { get; set; }
    public required string RowLabel { get; set; }
    public int SeatNumber { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }

    public static SeatType FromDto(SeatDto dto) => new()
    {
        Id = dto.Id,
        ScreenId = dto.ScreenId,
        RowLabel = dto.RowLabel,
        SeatNumber = dto.SeatNumber,
        Category = dto.Category,
        Price = dto.Price,
        IsAvailable = dto.IsAvailable
    };
}
