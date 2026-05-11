namespace MovieService.Core.Models;

public class MovieBooking
{
    public int Id { get; set; }
    public int ShowtimeId { get; set; }
    public Showtime Showtime { get; set; } = null!;
    public int UserId { get; set; }
    public string SeatNumbers { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime BookedAt { get; set; }
}
