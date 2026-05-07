using AdminBFF.Core.DTOs;
using AdminBFF.Core.Models;

namespace AdminBFF.Core.Services;

public interface IAdminService
{
    Task<UserDto> GetCurrentUserAsync(int userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<OperationResultDto> DeactivateUserAsync(int userId, int currentUserId);
    Task<List<BookingDto>> GetAllBookingsAsync(BookingFilterInput? filter);
    Task<OperationResultDto> CancelBookingAsync(int bookingId, string bookingType);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<TrainDto> AddTrainAsync(AddTrainInput input);
    Task<MovieDto> AddMovieAsync(AddMovieInput input);
}
