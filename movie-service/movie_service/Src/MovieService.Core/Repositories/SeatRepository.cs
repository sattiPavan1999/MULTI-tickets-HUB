using Microsoft.EntityFrameworkCore;
using MovieService.Core.Data;
using MovieService.Core.Models;

namespace MovieService.Core.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Seat>> GetSeatsByScreenAsync(int screenId)
    {
        return await _context.Seats
            .Where(s => s.ScreenId == screenId)
            .OrderBy(s => s.RowLabel)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();
    }

    public async Task<List<Seat>> GetSeatsByIdsAsync(int[] seatIds)
    {
        return await _context.Seats
            .Where(s => seatIds.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<List<int>> GetBookedSeatIdsForShowAsync(int showId)
    {
        var bookings = await _context.MovieBookings
            .Where(b => b.ShowId == showId && b.Status == "Confirmed")
            .ToListAsync();

        var bookedSeatIds = new List<int>();
        foreach (var booking in bookings)
        {
            bookedSeatIds.AddRange(booking.SelectedSeatIds);
        }

        return bookedSeatIds;
    }
}
