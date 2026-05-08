using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public class AdminService(
    IIdentityService identityService,
    IMovieService movieService,
    ITrainService trainService) : IAdminService
{
    public Task<List<UserDto>> GetAllUsersAsync(string bearerToken, CancellationToken ct = default)
        => identityService.GetAllUsersAsync(bearerToken, ct);

    public Task<List<MovieDto>> GetAllMoviesAsync(CancellationToken ct = default)
        => movieService.GetAllMoviesAsync(ct);

    public Task<List<TrainDto>> GetAllTrainsAsync(CancellationToken ct = default)
        => trainService.GetAllTrainsAsync(ct);
}
