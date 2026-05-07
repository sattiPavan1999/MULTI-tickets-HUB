using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IMovieService
{
    Task<List<MovieDto>> GetMoviesAsync(string? genre, string? language, string? format);
}
