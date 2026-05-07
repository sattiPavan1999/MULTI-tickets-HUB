using AdminBFF.Core.DTOs;
using AdminBFF.Core.Models;

namespace AdminBFF.Core.Services;

public interface ITrainService
{
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<Dictionary<string, int>> GetBookingStatsAsync();
    Task<OperationResultDto> CancelBookingAsync(int bookingId);
    Task<TrainDto> AddTrainAsync(AddTrainInput input);
}
