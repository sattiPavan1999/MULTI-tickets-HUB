using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public interface IShowtimeRepository : IBaseRepository<Showtime>
{
    Task<List<Showtime>> GetByMovieIdAsync(int movieId, CancellationToken ct = default);
    Task<Showtime?> GetByCompositeKeyAsync(int movieId, DateOnly date, TimeOnly time, string screen, CancellationToken ct = default);
    Task<List<Showtime>> GetByScreenAndDateAsync(string screen, DateOnly date, CancellationToken ct = default);
}
