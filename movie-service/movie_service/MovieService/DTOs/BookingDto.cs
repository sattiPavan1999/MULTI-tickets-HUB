namespace MovieService.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ShowId { get; set; }
    public required int[] SelectedSeatIds { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    public DateTime BookedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public ShowDto? Show { get; set; }
    public List<SeatDto>? Seats { get; set; }
}
