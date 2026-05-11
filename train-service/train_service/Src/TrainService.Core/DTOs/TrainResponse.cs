namespace TrainService.Core.DTOs;

public class TrainResponse
{
    public int Id { get; set; }
    public required string TrainName { get; set; }
    public required string TrainNumber { get; set; }
    public required string Source { get; set; }
    public required string Destination { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
