using MovieService.Core.DTOs;

namespace MovieService.Core.Services;

public interface IShowService
{
    Task<List<ShowDto>> GetShowsByMovieAsync(int movieId, DateTime? date);
}
