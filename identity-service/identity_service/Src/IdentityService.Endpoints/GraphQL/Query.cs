using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using IdentityService.Core.DTOs;
using IdentityService.Core.Services;

namespace IdentityService.Endpoints.GraphQL;

public class Query
{
    [Authorize]
    public async Task<UserType?> GetMe(ClaimsPrincipal claimsPrincipal, [Service] IAuthService authService)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        var user = await authService.GetUserByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        return user;
    }

    public async Task<UserType?> GetUser(int id, [Service] IAuthService authService)
    {
        return await authService.GetUserByIdAsync(id);
    }

    public async Task<List<UserType>> GetUsers([Service] IAuthService authService)
    {
        return await authService.GetAllUsersAsync();
    }

    public async Task<int> GetUserCount([Service] IAuthService authService)
    {
        return await authService.GetUserCountAsync();
    }
}
