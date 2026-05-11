using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(CreateBookingInput input);
}
