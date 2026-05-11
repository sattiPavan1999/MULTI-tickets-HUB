using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface IMovieService
{
    Task<List<MovieDto>> GetAllMoviesAsync();
    Task<MovieDto> CreateMovieAsync(CreateMovieRequest request);
    Task<MovieDto> UpdateMovieAsync(int id, UpdateMovieRequest request);
    Task DeleteMovieAsync(int id);
    Task<OperationResult> ToggleMovieStatusAsync(int id);
    Task<List<ShowtimeDto>> GetShowtimesAsync(int movieId);
    Task<ShowtimeDto> CreateShowtimeAsync(CreateShowtimeRequest request);
    Task DeleteShowtimeAsync(int id);
}
