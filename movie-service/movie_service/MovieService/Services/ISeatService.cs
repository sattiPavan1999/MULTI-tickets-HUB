using MovieService.DTOs;

namespace MovieService.Services;

public interface ISeatService
{
    Task<List<SeatDto>> GetSeatMapAsync(int showId);
}
