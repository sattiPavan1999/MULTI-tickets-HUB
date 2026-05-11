namespace TrainService.Core.DTOs;

public class CreateTrainBookingInput
{
    public int TrainId { get; set; }
    public int UserId { get; set; }
    public string TravelDate { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public int PassengerAge { get; set; }
    public int NumberOfSeats { get; set; }
}
