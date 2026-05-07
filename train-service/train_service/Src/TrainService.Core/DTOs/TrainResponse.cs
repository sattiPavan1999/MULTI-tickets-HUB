namespace TrainService.Core.DTOs;

public class TrainResponse
{
    public int Id { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string TrainName { get; set; } = string.Empty;
    public string SourceStation { get; set; } = string.Empty;
    public string DestinationStation { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public SeatAvailabilityDto AvailableSeats { get; set; } = new();
    public FareDto Fare { get; set; } = new();
}

public class SeatAvailabilityDto
{
    public int Sleeper { get; set; }
    public int Ac3Tier { get; set; }
    public int Ac2Tier { get; set; }
    public int Ac1Tier { get; set; }
}

public class FareDto
{
    public decimal Sleeper { get; set; }
    public decimal Ac3Tier { get; set; }
    public decimal Ac2Tier { get; set; }
    public decimal Ac1Tier { get; set; }
}
