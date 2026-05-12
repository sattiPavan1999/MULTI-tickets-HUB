using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(CreateBookingInput input);
    Task<List<BookingResponse>> GetMyBookingsAsync(int userId);
    Task<BookingResponse> GetBookingByIdAsync(int bookingId, int userId);
    Task<OperationResult> CancelBookingAsync(int bookingId, int userId);
}
