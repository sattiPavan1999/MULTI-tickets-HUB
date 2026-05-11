namespace MovieService.Core.Models;

public class Showtime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public DateOnly ShowDate { get; set; }
    public TimeOnly ShowTime { get; set; }
    public string ScreenNumber { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public DateTime CreatedAt { get; set; }
}
