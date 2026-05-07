using TrainService.Core.DTOs;

namespace TrainService.Core.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(CreateBookingInput input);
    Task<BookingResponse> GetBookingAsync(int bookingId, int userId);
    Task<CancelBookingResponse> CancelBookingAsync(int bookingId, int userId);
    Task<List<AdminBookingDto>> GetAllBookingsAsync();
    Task<BookingStatsDto> GetBookingStatsAsync();
}
