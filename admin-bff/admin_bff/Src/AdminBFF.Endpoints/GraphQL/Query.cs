using System.Security.Claims;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Models;
using AdminBFF.Core.Services;
using HotChocolate.Authorization;

namespace AdminBFF.Endpoints.GraphQL;

public class Query
{
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<UserDto> GetMe(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAdminService adminService)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Invalid or missing user ID in token");
        }

        return await adminService.GetCurrentUserAsync(userId);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<List<UserDto>> GetUsers([Service] IAdminService adminService)
    {
        return await adminService.GetAllUsersAsync();
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<List<BookingDto>> GetAllBookings(
        BookingFilterInput? filter,
        [Service] IAdminService adminService)
    {
        return await adminService.GetAllBookingsAsync(filter);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<DashboardStatsDto> GetDashboardStats([Service] IAdminService adminService)
    {
        return await adminService.GetDashboardStatsAsync();
    }
}
