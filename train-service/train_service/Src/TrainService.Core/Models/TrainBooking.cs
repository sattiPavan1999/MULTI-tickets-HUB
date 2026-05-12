namespace TrainService.Core.Models;

public class TrainBooking
{
    public int Id { get; set; }
    public int TrainId { get; set; }
    public Train Train { get; set; } = null!;
    public int UserId { get; set; }
    public DateOnly TravelDate { get; set; }
    public required string PassengerName { get; set; }
    public int PassengerAge { get; set; }
    public int NumberOfSeats { get; set; }
    public required string PNR { get; set; }
    public string Status { get; set; } = "Confirmed";
    public int? WaitlistPosition { get; set; }
    public DateTime BookedAt { get; set; }
    public string? BoardingStation { get; set; }
    public string? AlightingStation { get; set; }
}
