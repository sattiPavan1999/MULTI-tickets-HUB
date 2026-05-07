using Microsoft.EntityFrameworkCore;
using MovieService.Core.Data;
using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public class ShowRepository : IShowRepository
{
    private readonly AppDbContext _context;

    public ShowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Show>> GetShowsByMovieAsync(int movieId, DateTime? date)
    {
        var query = _context.Shows
            .Include(s => s.Screen)
                .ThenInclude(sc => sc.Cinema)
            .Where(s => s.MovieId == movieId);

        if (date.HasValue)
        {
            var startDate = date.Value.Date;
            var endDate = startDate.AddDays(1);
            query = query.Where(s => s.ShowTime >= startDate && s.ShowTime < endDate);
        }

        return await query.OrderBy(s => s.ShowTime).ToListAsync();
    }

    public async Task<Show?> GetShowByIdAsync(int id)
    {
        return await _context.Shows
            .Include(s => s.Screen)
                .ThenInclude(sc => sc.Cinema)
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task UpdateShowAsync(Show show)
    {
        _context.Shows.Update(show);
        await _context.SaveChangesAsync();
    }
}
