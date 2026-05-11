namespace MovieService.Core.DTOs;

public class CreateBookingInput
{
    public int ShowtimeId { get; set; }
    public int UserId { get; set; }
    public List<int> SeatNumbers { get; set; } = [];
}
