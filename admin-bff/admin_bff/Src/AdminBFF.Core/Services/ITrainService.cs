using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface ITrainService
{
    Task<List<TrainDto>> GetAllTrainsAsync(CancellationToken ct = default);
    Task<TrainDto> CreateTrainAsync(CreateTrainRequest request, CancellationToken ct = default);
    Task<TrainDto> UpdateTrainAsync(int id, UpdateTrainRequest request, CancellationToken ct = default);
    Task DeleteTrainAsync(int id, CancellationToken ct = default);
    Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(int trainId, CancellationToken ct = default);
    Task<SeatAvailabilityDto> UpdateSeatAvailabilityAsync(int trainId, UpdateSeatAvailabilityRequest request, CancellationToken ct = default);
}
