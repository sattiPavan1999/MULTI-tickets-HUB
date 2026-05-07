using MovieService.Core.DTOs;
using Microsoft.Extensions.Logging;
using MovieService.Core.Repositories;

namespace MovieService.Core.Services;

public class SeatServiceImpl : ISeatService
{
    private readonly ISeatRepository _seatRepository;
    private readonly IShowRepository _showRepository;
    private readonly ILogger<SeatServiceImpl> _logger;

    public SeatServiceImpl(
        ISeatRepository seatRepository,
        IShowRepository showRepository,
        ILogger<SeatServiceImpl> logger)
    {
        _seatRepository = seatRepository;
        _showRepository = showRepository;
        _logger = logger;
    }

    public async Task<List<SeatDto>> GetSeatMapAsync(int showId)
    {
        var show = await _showRepository.GetShowByIdAsync(showId);
        if (show == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Show with ID {showId} not found");
        }

        var seats = await _seatRepository.GetSeatsByScreenAsync(show.ScreenId);
        var bookedSeatIds = await _seatRepository.GetBookedSeatIdsForShowAsync(showId);

        return seats.Select(s => new SeatDto
        {
            Id = s.Id,
            ScreenId = s.ScreenId,
            RowLabel = s.RowLabel,
            SeatNumber = s.SeatNumber,
            Category = s.Category,
            Price = s.Price,
            IsAvailable = !bookedSeatIds.Contains(s.Id)
        }).ToList();
    }
}
