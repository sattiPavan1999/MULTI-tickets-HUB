using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public interface IShowtimeRepository : IBaseRepository<Showtime>
{
    Task<List<Showtime>> GetByMovieIdAsync(int movieId);
    Task<Showtime?> GetByCompositeKeyAsync(int movieId, DateOnly date, TimeOnly time, string screen);
    Task<List<Showtime>> GetByScreenAndDateAsync(string screen, DateOnly date);
}
