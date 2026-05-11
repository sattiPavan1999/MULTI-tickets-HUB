namespace MovieService.Core.DTOs;

public class SeatStatusResponse
{
    public int ShowtimeId { get; set; }
    public int TotalSeats { get; set; }
    public List<int> BookedSeats { get; set; } = [];
}
