using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public interface ISeatAvailabilityRepository
{
    Task<List<SeatAvailability>> GetByTrainAsync(int trainId, CancellationToken ct = default);
    Task<SeatAvailability?> GetByTrainAndDateAsync(int trainId, DateOnly date, CancellationToken ct = default);
    Task<SeatAvailability> UpsertAsync(SeatAvailability availability, CancellationToken ct = default);
}
