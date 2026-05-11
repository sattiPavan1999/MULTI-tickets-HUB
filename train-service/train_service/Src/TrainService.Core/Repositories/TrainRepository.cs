using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public class TrainRepository(TrainDbContext context, ILogger<TrainRepository> logger) : BaseRepository<Train>(context), ITrainRepository
{
    public async Task<Train?> GetByTrainNumberAsync(string trainNumber, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching train by number: {TrainNumber}", trainNumber);
        return await context.Trains.FirstOrDefaultAsync(t => t.TrainNumber == trainNumber, ct);
    }

    public async Task<List<Train>> GetAllAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Fetching all trains");
        return await context.Trains.ToListAsync(ct);
    }

    public async Task<List<Train>> SearchByRouteAsync(string? source, string? destination, string? sortBy, bool requiresAvailability = false, CancellationToken ct = default)
    {
        logger.LogDebug("Searching trains: source={Source}, destination={Destination}, sortBy={SortBy}, requiresAvailability={RequiresAvailability}", source, destination, sortBy, requiresAvailability);
        var query = context.Trains.AsNoTracking().AsQueryable();

        if (requiresAvailability)
            query = query.Where(t => context.SeatAvailabilities.Any(s => s.TrainId == t.Id));

        if (!string.IsNullOrWhiteSpace(source))
            query = query.Where(t => t.Source.ToLower().Contains(source.ToLower()));

        if (!string.IsNullOrWhiteSpace(destination))
            query = query.Where(t => t.Destination.ToLower().Contains(destination.ToLower()));

        query = sortBy switch
        {
            "departure" => query.OrderBy(t => t.DepartureTime),
            "price"     => query.OrderBy(t => t.Price),
            _           => query.OrderBy(t => t.Id)
        };

        return await query.ToListAsync(ct);
    }
}
