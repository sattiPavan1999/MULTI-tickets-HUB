using Microsoft.EntityFrameworkCore;
using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public class TrainBookingRepository(TrainDbContext context) : BaseRepository<TrainBooking>(context), ITrainBookingRepository
{
    public async Task<TrainBooking?> GetByPNRAsync(string pnr, CancellationToken ct = default)
        => await context.Bookings.FirstOrDefaultAsync(b => b.PNR == pnr, ct);

    public async Task<List<TrainBooking>> GetWaitlistedByTrainAndDateAsync(int trainId, DateOnly date, CancellationToken ct = default)
        => await context.Bookings
            .Where(b => b.TrainId == trainId && b.TravelDate == date && b.Status == "Waitlisted")
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync(ct);
}
