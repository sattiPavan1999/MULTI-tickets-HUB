using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IMovieService
{
    Task<List<MovieResponse>> GetAllMoviesAsync(bool? activeOnly = null);
    Task<MovieResponse?> GetMovieByIdAsync(int id);
    Task<MovieResponse> CreateMovieAsync(CreateMovieInput input);
    Task<MovieResponse> UpdateMovieAsync(int id, UpdateMovieInput input);
    Task DeleteMovieAsync(int id);
    Task<OperationResult> ToggleMovieStatusAsync(int id);
}
