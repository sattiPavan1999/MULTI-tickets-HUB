namespace AdminBFF.Core.DTOs;

public class SeatAvailabilityDto
{
    public int Id { get; set; }
    public int TrainId { get; set; }
    public string Date { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
}
