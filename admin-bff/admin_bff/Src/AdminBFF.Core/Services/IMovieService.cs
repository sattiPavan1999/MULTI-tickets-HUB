using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface IMovieService
{
    Task<List<MovieDto>> GetAllMoviesAsync(CancellationToken ct = default);
    Task<MovieDto> CreateMovieAsync(CreateMovieRequest request, CancellationToken ct = default);
    Task<MovieDto> UpdateMovieAsync(int id, UpdateMovieRequest request, CancellationToken ct = default);
    Task DeleteMovieAsync(int id, CancellationToken ct = default);
    Task<OperationResult> ToggleMovieStatusAsync(int id, CancellationToken ct = default);
}
