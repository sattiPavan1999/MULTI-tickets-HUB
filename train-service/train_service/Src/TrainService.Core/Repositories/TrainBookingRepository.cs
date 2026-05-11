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
}
