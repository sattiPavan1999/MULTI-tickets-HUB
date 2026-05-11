using Microsoft.EntityFrameworkCore;
using MovieService.Core.Data;
using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public class BookingRepository(MovieDbContext context)
    : BaseRepository<MovieBooking>(context), IBookingRepository
{
    public async Task<List<MovieBooking>> GetByShowtimeAsync(int showtimeId)
        => await context.Bookings
            .Where(b => b.ShowtimeId == showtimeId &&
                        (b.Status == "Pending" || b.Status == "Confirmed"))
            .ToListAsync();
}
