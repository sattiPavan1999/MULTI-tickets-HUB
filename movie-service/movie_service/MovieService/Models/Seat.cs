namespace MovieService.Models;

public class Seat
{
    public int Id { get; set; }
    public int ScreenId { get; set; }
    public required string RowLabel { get; set; }
    public int SeatNumber { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }

    // Navigation properties
    public Screen Screen { get; set; } = null!;
}
