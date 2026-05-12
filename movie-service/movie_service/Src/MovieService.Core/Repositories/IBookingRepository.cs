using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public interface IBookingRepository : IBaseRepository<MovieBooking>
{
    Task<List<MovieBooking>> GetByShowtimeAsync(int showtimeId);
    Task<List<MovieBooking>> GetByUserIdAsync(int userId);
    Task<MovieBooking?> GetByIdWithDetailsAsync(int bookingId);
}
