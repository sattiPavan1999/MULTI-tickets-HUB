using Microsoft.EntityFrameworkCore;
using MovieService.Data;
using MovieService.Models;

namespace MovieService.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _context;

    public MovieRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Movie>> GetMoviesAsync(string? genre, string? language, string? format)
    {
        var query = _context.Movies.AsQueryable();

        if (!string.IsNullOrEmpty(genre))
        {
            query = query.Where(m => m.Genre == genre);
        }

        if (!string.IsNullOrEmpty(language))
        {
            query = query.Where(m => m.Language == language);
        }

        if (!string.IsNullOrEmpty(format))
        {
            query = query.Where(m => m.Format == format);
        }

        return await query.ToListAsync();
    }

    public async Task<Movie?> GetMovieByIdAsync(int id)
    {
        return await _context.Movies.FindAsync(id);
    }
}
