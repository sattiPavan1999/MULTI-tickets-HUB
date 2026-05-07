using MovieService.Core.DTOs;

namespace MovieService.Endpoints.GraphQL.Types;

public class BookingType
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ShowId { get; set; }
    public required int[] SelectedSeatIds { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    public DateTime BookedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public ShowType? Show { get; set; }
    public List<SeatType>? Seats { get; set; }

    public static BookingType FromDto(BookingDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        ShowId = dto.ShowId,
        SelectedSeatIds = dto.SelectedSeatIds,
        TotalAmount = dto.TotalAmount,
        Status = dto.Status,
        BookedAt = dto.BookedAt,
        CancelledAt = dto.CancelledAt,
        Show = dto.Show == null ? null : ShowType.FromDto(dto.Show),
        Seats = dto.Seats?.Select(SeatType.FromDto).ToList()
    };
}
