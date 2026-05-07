using Microsoft.EntityFrameworkCore;
using MovieService.Core.Data;
using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MovieBooking> CreateBookingAsync(MovieBooking booking)
    {
        _context.MovieBookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<MovieBooking?> GetBookingByIdAsync(int id)
    {
        return await _context.MovieBookings.FindAsync(id);
    }

    public async Task<MovieBooking?> GetBookingWithDetailsAsync(int id)
    {
        return await _context.MovieBookings
            .Include(b => b.Show)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Show)
                .ThenInclude(s => s.Screen)
                    .ThenInclude(sc => sc.Cinema)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task UpdateBookingAsync(MovieBooking booking)
    {
        _context.MovieBookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MovieBooking>> GetAllAsync()
    {
        return await _context.MovieBookings
            .AsNoTracking()
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }
}
