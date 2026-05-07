using TrainService.Core.DTOs;

namespace TrainService.Core.Services;

public interface ITrainService
{
    Task<List<TrainResponse>> SearchTrainsAsync(SearchTrainInput input);
    Task<TrainResponse> GetTrainByIdAsync(int trainId, DateOnly travelDate);
}
