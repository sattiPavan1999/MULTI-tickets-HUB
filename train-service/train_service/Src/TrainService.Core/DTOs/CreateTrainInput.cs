namespace TrainService.Core.DTOs;

public class CreateTrainInput
{
    public string TrainName { get; set; } = string.Empty;
    public string TrainNumber { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
}
