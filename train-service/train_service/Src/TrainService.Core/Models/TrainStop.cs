namespace TrainService.Core.Models;

public class TrainStop
{
    public int Id { get; set; }
    public int TrainId { get; set; }
    public Train Train { get; set; } = null!;
    public int StopNumber { get; set; }
    public required string StationName { get; set; }
}
