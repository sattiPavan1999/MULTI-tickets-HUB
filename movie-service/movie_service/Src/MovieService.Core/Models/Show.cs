namespace MovieService.Core.Models;

public class Show
{
    public int Id { get; set; }
    public int ScreenId { get; set; }
    public int MovieId { get; set; }
    public DateTime ShowTime { get; set; }
    public int AvailableSeats { get; set; }

    // Navigation properties
    public Screen Screen { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
    public ICollection<MovieBooking> Bookings { get; set; } = new List<MovieBooking>();
}
