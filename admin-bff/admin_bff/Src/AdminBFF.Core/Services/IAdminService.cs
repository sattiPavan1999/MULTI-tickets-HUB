using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface IAdminService
{
    Task<List<UserDto>> GetAllUsersAsync(string bearerToken);
    Task<List<MovieDto>> GetAllMoviesAsync();
    Task<List<TrainDto>> GetAllTrainsAsync();
}
