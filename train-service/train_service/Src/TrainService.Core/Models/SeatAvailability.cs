namespace TrainService.Core.Models;

public class SeatAvailability
{
    public int Id { get; set; }
    public int TrainId { get; set; }
    public Train Train { get; set; } = null!;
    public DateOnly Date { get; set; }
    public int AvailableSeats { get; set; }
}
