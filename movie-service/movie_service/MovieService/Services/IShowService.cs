using MovieService.DTOs;

namespace MovieService.Services;

public interface IShowService
{
    Task<List<ShowDto>> GetShowsByMovieAsync(int movieId, DateTime? date);
}
