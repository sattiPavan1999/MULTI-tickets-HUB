using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface IIdentityService
{
    Task<List<UserDto>> GetAllUsersAsync(string bearerToken, CancellationToken ct = default);
    Task<OperationResult> ToggleUserStatusAsync(int userId, string bearerToken, CancellationToken ct = default);
}
