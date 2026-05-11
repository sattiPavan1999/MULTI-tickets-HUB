using Microsoft.EntityFrameworkCore;
using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public class SeatAvailabilityRepository(TrainDbContext context) : ISeatAvailabilityRepository
{
    public async Task<List<SeatAvailability>> GetByTrainAsync(int trainId, CancellationToken ct = default)
        => await context.SeatAvailabilities.Where(s => s.TrainId == trainId).ToListAsync(ct);

    public async Task<SeatAvailability?> GetByTrainAndDateAsync(int trainId, DateOnly date, CancellationToken ct = default)
        => await context.SeatAvailabilities.FirstOrDefaultAsync(s => s.TrainId == trainId && s.Date == date, ct);

    public async Task<SeatAvailability> UpsertAsync(SeatAvailability availability, CancellationToken ct = default)
    {
        // Check change tracker first — if the entity was loaded earlier in the same unit of work
        // (e.g. from GetByTrainAndDateAsync in the booking transaction) it's already tracked
        // and we avoid a redundant SELECT.
        var tracked = context.ChangeTracker.Entries<SeatAvailability>()
            .FirstOrDefault(e => e.Entity.TrainId == availability.TrainId && e.Entity.Date == availability.Date);

        if (tracked is not null)
        {
            tracked.Entity.AvailableSeats = availability.AvailableSeats;
            await context.SaveChangesAsync(ct);
            return tracked.Entity;
        }

        var existing = await GetByTrainAndDateAsync(availability.TrainId, availability.Date, ct);
        if (existing is null)
        {
            await context.SeatAvailabilities.AddAsync(availability, ct);
        }
        else
        {
            existing.AvailableSeats = availability.AvailableSeats;
            context.SeatAvailabilities.Update(existing);
            availability = existing;
        }
        await context.SaveChangesAsync(ct);
        return availability;
    }
}
