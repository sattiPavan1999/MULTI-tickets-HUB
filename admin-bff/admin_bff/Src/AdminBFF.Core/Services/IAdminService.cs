using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface IAdminService
{
    Task<List<UserDto>> GetAllUsersAsync(string bearerToken, CancellationToken ct = default);
    Task<List<MovieDto>> GetAllMoviesAsync(CancellationToken ct = default);
    Task<List<TrainDto>> GetAllTrainsAsync(CancellationToken ct = default);
}
