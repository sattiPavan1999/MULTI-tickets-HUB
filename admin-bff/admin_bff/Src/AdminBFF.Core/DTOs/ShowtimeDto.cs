namespace AdminBFF.Core.DTOs;

public class ShowtimeDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string ShowDate { get; set; } = string.Empty;
    public string ShowTime { get; set; } = string.Empty;
    public string ScreenNumber { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
