using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public interface ISeatRepository
{
    Task<List<Seat>> GetSeatsByScreenAsync(int screenId);
    Task<List<Seat>> GetSeatsByIdsAsync(int[] seatIds);
    Task<List<int>> GetBookedSeatIdsForShowAsync(int showId);
}
