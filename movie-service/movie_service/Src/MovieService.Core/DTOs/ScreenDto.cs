namespace MovieService.Core.DTOs;

public class ScreenDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int TotalSeats { get; set; }
    public CinemaDto? Cinema { get; set; }
}
