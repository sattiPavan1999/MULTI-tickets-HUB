using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IShowtimeService
{
    Task<List<ShowtimeResponse>> GetShowtimesByMovieAsync(int movieId);
    Task<ShowtimeResponse> CreateShowtimeAsync(CreateShowtimeInput input);
    Task DeleteShowtimeAsync(int id);
    Task<SeatStatusResponse> GetSeatStatusAsync(int showtimeId);
}
