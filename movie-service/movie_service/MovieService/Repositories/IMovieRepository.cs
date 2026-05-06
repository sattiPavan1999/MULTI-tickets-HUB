using MovieService.Models;

namespace MovieService.Repositories;

public interface IMovieRepository
{
    Task<List<Movie>> GetMoviesAsync(string? genre, string? language, string? format);
    Task<Movie?> GetMovieByIdAsync(int id);
}
