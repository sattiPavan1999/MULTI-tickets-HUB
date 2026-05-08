using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public interface IMovieRepository : IBaseRepository<Movie>
{
    Task<List<Movie>> GetAllAsync(CancellationToken ct = default);
}
