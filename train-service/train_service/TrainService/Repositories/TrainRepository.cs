using Microsoft.EntityFrameworkCore;
using TrainService.Data;
using TrainService.Models;

namespace TrainService.Repositories;

public class TrainRepository : ITrainRepository
{
    private readonly TrainDbContext _context;
    private readonly ILogger<TrainRepository> _logger;

    public TrainRepository(TrainDbContext context, ILogger<TrainRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Train>> SearchTrainsAsync(string sourceStation, string destinationStation)
    {
        _logger.LogInformation("Searching trains from {Source} to {Destination}", sourceStation, destinationStation);

        return await _context.Trains
            .Where(t => t.SourceStation == sourceStation && t.DestinationStation == destinationStation)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Train?> GetTrainByIdAsync(int trainId)
    {
        _logger.LogInformation("Fetching train with ID: {TrainId}", trainId);

        return await _context.Trains
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == trainId);
    }

    public async Task<Train?> GetTrainByNumberAsync(string trainNumber)
    {
        _logger.LogInformation("Fetching train with number: {TrainNumber}", trainNumber);

        return await _context.Trains
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TrainNumber == trainNumber);
    }
}
