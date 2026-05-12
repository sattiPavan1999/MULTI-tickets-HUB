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
    public string? TrainName { get; set; }
    public string? TrainNumber { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
}
