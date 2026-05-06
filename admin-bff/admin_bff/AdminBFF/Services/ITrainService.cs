using AdminBFF.DTOs;
using AdminBFF.Models;

namespace AdminBFF.Services;

public interface ITrainService
{
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<Dictionary<string, int>> GetBookingStatsAsync();
    Task<OperationResultDto> CancelBookingAsync(int bookingId);
    Task<TrainDto> AddTrainAsync(AddTrainInput input);
}
