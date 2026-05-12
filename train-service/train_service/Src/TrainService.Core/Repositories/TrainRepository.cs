using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public class TrainRepository(TrainDbContext context, ILogger<TrainRepository> logger) : BaseRepository<Train>(context), ITrainRepository
{
    public async Task<Train?> GetByTrainNumberAsync(string trainNumber)
    {
        logger.LogDebug("Fetching train by number: {TrainNumber}", trainNumber);
        return await context.Trains.FirstOrDefaultAsync(t => t.TrainNumber == trainNumber);
    }

    public async Task<Train?> GetByIdWithStopsAsync(int id)
        => await context.Trains
            .Include(t => t.Stops.OrderBy(s => s.StopNumber))
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<Train>> GetAllAsync()
    {
        logger.LogDebug("Fetching all trains");
        return await context.Trains
            .Include(t => t.Stops.OrderBy(s => s.StopNumber))
            .ToListAsync();
    }

    public async Task<List<Train>> SearchByRouteAsync(string? source, string? destination, string? sortBy, bool requiresAvailability = false)
    {
        logger.LogDebug("Searching trains: source={Source}, destination={Destination}, sortBy={SortBy}, requiresAvailability={RequiresAvailability}", source, destination, sortBy, requiresAvailability);

        IQueryable<Train> query;

        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(destination))
        {
            var srcLower = source.ToLower();
            var dstLower = destination.ToLower();
            query = context.Trains.AsNoTracking()
                .Where(t =>
                    t.Stops.Any(s => s.StationName.ToLower().Contains(srcLower)) &&
                    t.Stops.Any(s => s.StationName.ToLower().Contains(dstLower)) &&
                    t.Stops.Where(s => s.StationName.ToLower().Contains(srcLower)).Min(s => s.StopNumber) <
                    t.Stops.Where(s => s.StationName.ToLower().Contains(dstLower)).Max(s => s.StopNumber));
        }
        else if (!string.IsNullOrWhiteSpace(source))
        {
            var srcLower = source.ToLower();
            query = context.Trains.AsNoTracking()
                .Where(t => t.Stops.Any(s => s.StationName.ToLower().Contains(srcLower)));
        }
        else if (!string.IsNullOrWhiteSpace(destination))
        {
            var dstLower = destination.ToLower();
            query = context.Trains.AsNoTracking()
                .Where(t => t.Stops.Any(s => s.StationName.ToLower().Contains(dstLower)));
        }
        else
        {
            query = context.Trains.AsNoTracking();
        }

        if (requiresAvailability)
            query = query.Where(t => context.SeatAvailabilities.Any(s => s.TrainId == t.Id));

        query = query.Include(t => t.Stops.OrderBy(s => s.StopNumber));

        query = sortBy switch
        {
            "departure" => query.OrderBy(t => t.DepartureTime),
            "price"     => query.OrderBy(t => t.Price),
            _           => query.OrderBy(t => t.Id)
        };

        return await query.ToListAsync();
    }
}
