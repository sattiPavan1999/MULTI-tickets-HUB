namespace MovieService.Models;

public class Screen
{
    public int Id { get; set; }
    public int CinemaId { get; set; }
    public required string Name { get; set; }
    public int TotalSeats { get; set; }

    // Navigation properties
    public Cinema Cinema { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Show> Shows { get; set; } = new List<Show>();
}
