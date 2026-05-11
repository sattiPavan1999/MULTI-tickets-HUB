using TrainService.Core.DTOs;

namespace TrainService.Core.Services;

public interface ITrainService
{
    Task<List<TrainResponse>> GetAllTrainsAsync();
    Task<List<TrainResponse>> SearchTrainsAsync(string? source, string? destination, string? sortBy, bool requiresAvailability = false);
    Task<TrainResponse?> GetTrainByIdAsync(int id);
    Task<TrainResponse> CreateTrainAsync(CreateTrainInput input);
    Task<TrainResponse> UpdateTrainAsync(int id, UpdateTrainInput input);
    Task DeleteTrainAsync(int id);
    Task<List<SeatAvailabilityResponse>> GetSeatAvailabilityAsync(int trainId);
    Task<SeatAvailabilityResponse> UpdateSeatAvailabilityAsync(int trainId, SeatAvailabilityInput input);
}
