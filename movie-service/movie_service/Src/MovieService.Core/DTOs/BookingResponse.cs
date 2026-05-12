namespace MovieService.Core.DTOs;

public class BookingResponse
{
    public int Id { get; set; }
    public int ShowtimeId { get; set; }
    public int UserId { get; set; }
    public string SeatNumbers { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
    public string? MovieTitle { get; set; }
    public string? ShowDate { get; set; }
    public string? ShowTime { get; set; }
    public string? ScreenNumber { get; set; }
}
