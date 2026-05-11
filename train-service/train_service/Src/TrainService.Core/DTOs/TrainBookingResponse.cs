namespace TrainService.Core.DTOs;

public class TrainBookingResponse
{
    public int Id { get; set; }
    public int TrainId { get; set; }
    public int UserId { get; set; }
    public DateOnly TravelDate { get; set; }
    public required string PassengerName { get; set; }
    public int PassengerAge { get; set; }
    public int NumberOfSeats { get; set; }
    public required string PNR { get; set; }
    public required string Status { get; set; }
    public int? WaitlistPosition { get; set; }
    public DateTime BookedAt { get; set; }
}
