using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IMovieService
{
    Task<List<MovieResponse>> GetAllMoviesAsync(CancellationToken ct = default);
    Task<MovieResponse?> GetMovieByIdAsync(int id, CancellationToken ct = default);
    Task<MovieResponse> CreateMovieAsync(CreateMovieInput input, CancellationToken ct = default);
    Task<MovieResponse> UpdateMovieAsync(int id, UpdateMovieInput input, CancellationToken ct = default);
    Task DeleteMovieAsync(int id, CancellationToken ct = default);
    Task<OperationResult> ToggleMovieStatusAsync(int id, CancellationToken ct = default);
}
