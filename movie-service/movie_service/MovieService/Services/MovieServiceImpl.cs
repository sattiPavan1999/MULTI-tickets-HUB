using MovieService.DTOs;
using MovieService.Repositories;

namespace MovieService.Services;

public class MovieServiceImpl : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<MovieServiceImpl> _logger;

    public MovieServiceImpl(IMovieRepository movieRepository, ILogger<MovieServiceImpl> logger)
    {
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<List<MovieDto>> GetMoviesAsync(string? genre, string? language, string? format)
    {
        if (!string.IsNullOrEmpty(format) && format != "2D" && format != "3D" && format != "IMAX")
        {
            throw new ArgumentException("Invalid format. Must be one of: 2D, 3D, IMAX");
        }

        var movies = await _movieRepository.GetMoviesAsync(genre, language, format);

        return movies.Select(m => new MovieDto
        {
            Id = m.Id,
            Title = m.Title,
            Genre = m.Genre,
            Language = m.Language,
            Format = m.Format,
            DurationMinutes = m.DurationMinutes,
            Synopsis = m.Synopsis,
            PosterUrl = m.PosterUrl
        }).ToList();
    }
}
