namespace AdminBFF.Core.DTOs;

public record BookingDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required string BookingType { get; init; } // "Train" or "Movie"
    public int? Pnr { get; init; } // Present only for Train bookings
    public int? ShowId { get; init; } // Present only for Movie bookings
    public decimal TotalAmount { get; init; }
    public required string Status { get; init; } // "Confirmed" or "Cancelled"
    public DateTime BookedAt { get; init; }
}
