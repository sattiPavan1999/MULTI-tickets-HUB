namespace AdminBFF.Core.DTOs;

public class CreateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
}

public class UpdateMovieRequest
{
    public string? Title { get; set; }
    public string? Genre { get; set; }
    public int? Duration { get; set; }
    public string? PosterUrl { get; set; }
}

public class CreateTrainRequest
{
    public string TrainName { get; set; } = string.Empty;
    public string TrainNumber { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
}

public class UpdateTrainRequest
{
    public string? TrainName { get; set; }
    public string? TrainNumber { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public DateTime? DepartureTime { get; set; }
}

public class UpdateSeatAvailabilityRequest
{
    public string Date { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
}

public class CreateShowtimeRequest
{
    public int MovieId { get; set; }
    public string ShowDate { get; set; } = string.Empty;
    public string ShowTime { get; set; } = string.Empty;
    public string ScreenNumber { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}
