using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IBookingService
{
    Task<BookingDto> BookSeatsAsync(int userId, int showId, int[] selectedSeatIds);
    Task<BookingDto> CancelBookingAsync(int bookingId, int userId);
    Task<BookingDto> GetBookingAsync(int bookingId, int userId);
    Task<List<AdminBookingDto>> GetAllBookingsAsync();
    Task<BookingStatsDto> GetBookingStatsAsync();
}
