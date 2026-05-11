using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IShowtimeService
{
    Task<List<ShowtimeResponse>> GetShowtimesByMovieAsync(int movieId, CancellationToken ct = default);
    Task<ShowtimeResponse> CreateShowtimeAsync(CreateShowtimeInput input, CancellationToken ct = default);
    Task DeleteShowtimeAsync(int id, CancellationToken ct = default);
    Task<SeatStatusResponse> GetSeatStatusAsync(int showtimeId, CancellationToken ct = default);
}
