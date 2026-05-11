using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieService.Core.Data;
using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public class ShowtimeRepository(MovieDbContext context, ILogger<ShowtimeRepository> logger)
    : BaseRepository<Showtime>(context), IShowtimeRepository
{
    public async Task<List<Showtime>> GetByMovieIdAsync(int movieId)
    {
        logger.LogDebug("Fetching showtimes for movie {MovieId}", movieId);
        return await context.Showtimes
            .Where(s => s.MovieId == movieId)
            .OrderBy(s => s.ShowDate)
            .ThenBy(s => s.ShowTime)
            .ToListAsync();
    }

    public async Task<Showtime?> GetByCompositeKeyAsync(int movieId, DateOnly date, TimeOnly time, string screen)
        => await context.Showtimes
            .FirstOrDefaultAsync(s =>
                s.MovieId == movieId &&
                s.ShowDate == date &&
                s.ShowTime == time &&
                s.ScreenNumber == screen);

    public async Task<List<Showtime>> GetByScreenAndDateAsync(string screen, DateOnly date)
        => await context.Showtimes
            .Where(s => s.ScreenNumber == screen && s.ShowDate == date)
            .ToListAsync();
}
