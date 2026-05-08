using TrainService.Core.DTOs;

namespace TrainService.Core.Services;

public interface ITrainService
{
    Task<List<TrainResponse>> GetAllTrainsAsync(CancellationToken ct = default);
    Task<TrainResponse?> GetTrainByIdAsync(int id, CancellationToken ct = default);
    Task<TrainResponse> CreateTrainAsync(CreateTrainInput input, CancellationToken ct = default);
    Task<TrainResponse> UpdateTrainAsync(int id, UpdateTrainInput input, CancellationToken ct = default);
    Task DeleteTrainAsync(int id, CancellationToken ct = default);
    Task<List<SeatAvailabilityResponse>> GetSeatAvailabilityAsync(int trainId, CancellationToken ct = default);
    Task<SeatAvailabilityResponse> UpdateSeatAvailabilityAsync(int trainId, SeatAvailabilityInput input, CancellationToken ct = default);
}
