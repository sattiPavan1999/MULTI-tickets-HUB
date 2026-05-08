namespace TrainService.Core.DTOs;

public class SeatAvailabilityResponse
{
    public int Id { get; set; }
    public int TrainId { get; set; }
    public DateOnly Date { get; set; }
    public int AvailableSeats { get; set; }
}
