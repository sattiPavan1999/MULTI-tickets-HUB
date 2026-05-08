namespace TrainService.Core.DTOs;

public class SeatAvailabilityInput
{
    public DateOnly Date { get; set; }
    public int AvailableSeats { get; set; }
}
