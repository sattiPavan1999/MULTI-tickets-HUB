namespace MovieService.Core.DTOs;

public class ShowDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int ScreenId { get; set; }
    public DateTime ShowTime { get; set; }
    public int AvailableSeats { get; set; }
    public ScreenDto? Screen { get; set; }
}
