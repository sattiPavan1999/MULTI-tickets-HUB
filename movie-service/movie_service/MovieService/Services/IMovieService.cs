using MovieService.DTOs;

namespace MovieService.Services;

public interface IMovieService
{
    Task<List<MovieDto>> GetMoviesAsync(string? genre, string? language, string? format);
}
