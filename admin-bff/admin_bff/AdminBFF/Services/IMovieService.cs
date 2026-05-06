using AdminBFF.DTOs;
using AdminBFF.Models;

namespace AdminBFF.Services;

public interface IMovieService
{
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<Dictionary<string, int>> GetBookingStatsAsync();
    Task<OperationResultDto> CancelBookingAsync(int bookingId);
    Task<MovieDto> AddMovieAsync(AddMovieInput input);
}
