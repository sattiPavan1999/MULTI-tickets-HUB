namespace MovieService.Models;

public class MovieBooking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ShowId { get; set; }
    public required int[] SelectedSeatIds { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    public DateTime BookedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public Show Show { get; set; } = null!;
}
