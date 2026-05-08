using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieService.Core.Data;
using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public class MovieRepository(MovieDbContext context, ILogger<MovieRepository> logger) : BaseRepository<Movie>(context), IMovieRepository
{
    public async Task<List<Movie>> GetAllAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Fetching all movies");
        return await context.Movies.ToListAsync(ct);
    }
}
