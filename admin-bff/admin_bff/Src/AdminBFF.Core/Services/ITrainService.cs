using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface ITrainService
{
    Task<List<TrainDto>> GetAllTrainsAsync();
    Task<TrainDto> CreateTrainAsync(CreateTrainRequest request);
    Task<TrainDto> UpdateTrainAsync(int id, UpdateTrainRequest request);
    Task DeleteTrainAsync(int id);
    Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(int trainId);
    Task<SeatAvailabilityDto> UpdateSeatAvailabilityAsync(int trainId, UpdateSeatAvailabilityRequest request);
}
