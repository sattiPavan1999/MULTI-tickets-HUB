namespace TrainService.Core.DTOs;

public class UpdateTrainInput
{
    public string? TrainName { get; set; }
    public string? TrainNumber { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public DateTime? DepartureTime { get; set; }
}
