using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public interface IShowRepository
{
    Task<List<Show>> GetShowsByMovieAsync(int movieId, DateTime? date);
    Task<Show?> GetShowByIdAsync(int id);
    Task UpdateShowAsync(Show show);
}
