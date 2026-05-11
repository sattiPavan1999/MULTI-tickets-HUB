using TrainService.Core.DTOs;

namespace TrainService.Core.Services;

public interface ITrainBookingService
{
    Task<TrainBookingResponse> CreateBookingAsync(CreateTrainBookingInput input, CancellationToken ct = default);
    Task PromoteWaitlistAsync(int trainId, DateOnly date, CancellationToken ct = default);
}
