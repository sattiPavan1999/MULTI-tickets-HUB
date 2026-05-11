namespace MovieService.Core.DTOs;

public class CreateShowtimeInput
{
    public int MovieId { get; set; }
    public string ShowDate { get; set; } = string.Empty;
    public string ShowTime { get; set; } = string.Empty;
    public string ScreenNumber { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}
