namespace AdminBFF.Models;

public record AddTrainInput
{
    public required string TrainNumber { get; init; }
    public required string TrainName { get; init; }
    public required string SourceStation { get; init; }
    public required string DestinationStation { get; init; }
    public required string DepartureTime { get; init; }
    public required string ArrivalTime { get; init; }
    public required Dictionary<string, int> TotalSeats { get; init; }
}
