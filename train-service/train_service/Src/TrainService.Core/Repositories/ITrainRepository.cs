using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public interface ITrainRepository : IBaseRepository<Train>
{
    Task<Train?> GetByTrainNumberAsync(string trainNumber);
    Task<List<Train>> GetAllAsync();
    Task<List<Train>> SearchByRouteAsync(string? source, string? destination, string? sortBy, bool requiresAvailability = false);
}
