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
}
