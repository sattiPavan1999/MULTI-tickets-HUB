namespace AdminBFF.Models;

public record BookingFilterInput
{
    public string? Status { get; init; } // "Confirmed" or "Cancelled"
    public string? ServiceType { get; init; } // "Train" or "Movie"
}
