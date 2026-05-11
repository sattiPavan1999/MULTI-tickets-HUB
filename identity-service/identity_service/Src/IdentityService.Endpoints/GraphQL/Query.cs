using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using IdentityService.Core.DTOs;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using IdentityService.Core.Services;

namespace IdentityService.Endpoints.GraphQL;

public class Query
{
    [Authorize]
    public async Task<UserType?> GetMe(
        ClaimsPrincipal claimsPrincipal,
        [Service] IUserAccountService userAccountService,
        CancellationToken ct)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid token");

        return await userAccountService.GetUserByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found");
    }

    [Authorize]
    public async Task<UserType?> GetUser(int id, [Service] IUserAccountService userAccountService, CancellationToken ct)
        => await userAccountService.GetUserByIdAsync(id, ct);

    [Authorize]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] IUserRepository userRepository)
        => userRepository.Query();

    [Authorize]
    public async Task<int> GetUserCount([Service] IUserAccountService userAccountService, CancellationToken ct)
        => await userAccountService.GetUserCountAsync(ct);
}
