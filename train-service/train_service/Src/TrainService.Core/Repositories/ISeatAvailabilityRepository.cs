using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public interface ISeatAvailabilityRepository
{
    Task<List<SeatAvailability>> GetByTrainAsync(int trainId);
    Task<SeatAvailability?> GetByTrainAndDateAsync(int trainId, DateOnly date);
    Task<SeatAvailability> UpsertAsync(SeatAvailability availability);
}
