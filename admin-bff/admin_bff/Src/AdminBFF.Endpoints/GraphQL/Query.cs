using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using HotChocolate.Authorization;

namespace AdminBFF.Endpoints.GraphQL;

[Authorize(Roles = new[] { "Admin" })]
public class Query
{
    public async Task<List<UserDto>> GetUsers(
        [Service] IIdentityService identityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        CancellationToken ct)
    {
        var token = ExtractToken(httpContextAccessor);
        return await identityService.GetAllUsersAsync(token, ct);
    }

    public async Task<List<MovieDto>> GetMovies(
        [Service] IMovieService movieService,
        CancellationToken ct)
        => await movieService.GetAllMoviesAsync(ct);

    public async Task<List<TrainDto>> GetTrains(
        [Service] ITrainService trainService,
        CancellationToken ct)
        => await trainService.GetAllTrainsAsync(ct);

    private static string ExtractToken(IHttpContextAccessor accessor)
    {
        var authHeader = accessor.HttpContext?.Request.Headers.Authorization.ToString();
        return authHeader?.StartsWith("Bearer ") == true ? authHeader[7..] : string.Empty;
    }
}
