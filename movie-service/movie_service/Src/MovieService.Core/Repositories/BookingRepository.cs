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

    public async Task<List<MovieBooking>> GetByUserIdAsync(int userId)
        => await context.Bookings
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();

    public async Task<MovieBooking?> GetByIdWithDetailsAsync(int bookingId)
        => await context.Bookings
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
}
