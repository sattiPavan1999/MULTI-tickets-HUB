using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public interface ITrainRepository : IBaseRepository<Train>
{
    Task<Train?> GetByTrainNumberAsync(string trainNumber, CancellationToken ct = default);
    Task<List<Train>> GetAllAsync(CancellationToken ct = default);
    Task<List<Train>> SearchByRouteAsync(string? source, string? destination, string? sortBy, bool requiresAvailability = false, CancellationToken ct = default);
}
