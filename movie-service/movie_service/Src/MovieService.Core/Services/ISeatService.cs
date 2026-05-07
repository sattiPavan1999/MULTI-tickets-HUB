using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface ISeatService
{
    Task<List<SeatDto>> GetSeatMapAsync(int showId);
}
