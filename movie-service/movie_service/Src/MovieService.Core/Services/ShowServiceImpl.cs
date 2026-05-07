using MovieService.Core.DTOs;
using Microsoft.Extensions.Logging;
using MovieService.Core.Repositories;

namespace MovieService.Core.Services;

public class ShowServiceImpl : IShowService
{
    private readonly IShowRepository _showRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<ShowServiceImpl> _logger;

    public ShowServiceImpl(
        IShowRepository showRepository,
        IMovieRepository movieRepository,
        ILogger<ShowServiceImpl> logger)
    {
        _showRepository = showRepository;
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<List<ShowDto>> GetShowsByMovieAsync(int movieId, DateTime? date)
    {
        var movie = await _movieRepository.GetMovieByIdAsync(movieId);
        if (movie == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Movie with ID {movieId} not found");
        }

        if (date.HasValue && date.Value.Date < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Date must be current date or future date");
        }

        var shows = await _showRepository.GetShowsByMovieAsync(movieId, date);

        return shows.Select(s => new ShowDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            ScreenId = s.ScreenId,
            ShowTime = s.ShowTime,
            AvailableSeats = s.AvailableSeats,
            Screen = s.Screen == null ? null : new ScreenDto
            {
                Id = s.Screen.Id,
                Name = s.Screen.Name,
                TotalSeats = s.Screen.TotalSeats,
                Cinema = s.Screen.Cinema == null ? null : new CinemaDto
                {
                    Id = s.Screen.Cinema.Id,
                    Name = s.Screen.Cinema.Name,
                    City = s.Screen.Cinema.City,
                    Address = s.Screen.Cinema.Address
                }
            }
        }).ToList();
    }
}
