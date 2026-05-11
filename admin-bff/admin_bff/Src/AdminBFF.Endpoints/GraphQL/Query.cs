using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using HotChocolate.Authorization;

namespace AdminBFF.Endpoints.GraphQL;

[Authorize(Roles = new[] { "Admin" })]
public class Query
{
    public async Task<List<UserDto>> GetUsers(
        [Service] IIdentityService identityService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var token = ExtractToken(httpContextAccessor);
        return await identityService.GetAllUsersAsync(token);
    }

    public async Task<List<MovieDto>> GetMovies([Service] IMovieService movieService)
        => await movieService.GetAllMoviesAsync();

    public async Task<List<TrainDto>> GetTrains([Service] ITrainService trainService)
        => await trainService.GetAllTrainsAsync();

    private static string ExtractToken(IHttpContextAccessor accessor)
    {
        var authHeader = accessor.HttpContext?.Request.Headers.Authorization.ToString();
        return authHeader?.StartsWith("Bearer ") == true ? authHeader[7..] : string.Empty;
    }
}
