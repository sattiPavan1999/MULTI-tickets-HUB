namespace MovieService.Core.DTOs;

public class AdminBookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ShowId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
}

public class BookingStatsDto
{
    public int Total { get; set; }
    public int Cancelled { get; set; }
}
