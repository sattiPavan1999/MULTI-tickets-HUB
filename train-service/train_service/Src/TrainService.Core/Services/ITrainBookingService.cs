using TrainService.Core.DTOs;

namespace TrainService.Core.Services;

public interface ITrainBookingService
{
    Task<TrainBookingResponse> CreateBookingAsync(CreateTrainBookingInput input);
    Task PromoteWaitlistAsync(int trainId, DateOnly date);
    Task<OperationResult> CancelBookingAsync(int bookingId);
}
