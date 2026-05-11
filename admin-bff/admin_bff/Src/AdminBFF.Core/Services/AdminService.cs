using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public class AdminService(
    IIdentityService identityService,
    IMovieService movieService,
    ITrainService trainService) : IAdminService
{
    public Task<List<UserDto>> GetAllUsersAsync(string bearerToken)
        => identityService.GetAllUsersAsync(bearerToken);

    public Task<List<MovieDto>> GetAllMoviesAsync()
        => movieService.GetAllMoviesAsync();

    public Task<List<TrainDto>> GetAllTrainsAsync()
        => trainService.GetAllTrainsAsync();
}
