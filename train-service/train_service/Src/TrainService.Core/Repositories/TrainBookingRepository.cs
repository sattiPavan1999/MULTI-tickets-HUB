using Microsoft.EntityFrameworkCore;
using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public class TrainBookingRepository(TrainDbContext context) : BaseRepository<TrainBooking>(context), ITrainBookingRepository
{
    public async Task<TrainBooking?> GetByPNRAsync(string pnr)
        => await context.Bookings.FirstOrDefaultAsync(b => b.PNR == pnr);

    public async Task<List<TrainBooking>> GetWaitlistedByTrainAndDateAsync(int trainId, DateOnly date)
        => await context.Bookings
            .Where(b => b.TrainId == trainId && b.TravelDate == date && b.Status == "Waitlisted")
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync();

    public async Task<List<TrainBooking>> GetByUserIdAsync(int userId)
        => await context.Bookings
            .Include(b => b.Train)
                .ThenInclude(t => t.Stops.OrderBy(s => s.StopNumber))
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();

    public async Task<TrainBooking?> GetByIdWithDetailsAsync(int bookingId)
        => await context.Bookings
            .Include(b => b.Train)
                .ThenInclude(t => t.Stops.OrderBy(s => s.StopNumber))
            .FirstOrDefaultAsync(b => b.Id == bookingId);
}
