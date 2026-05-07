using AdminBFF.Core.DTOs;
using AdminBFF.Core.Models;

namespace AdminBFF.Core.Services;

public interface IMovieService
{
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<Dictionary<string, int>> GetBookingStatsAsync();
    Task<OperationResultDto> CancelBookingAsync(int bookingId);
    Task<MovieDto> AddMovieAsync(AddMovieInput input);
}
