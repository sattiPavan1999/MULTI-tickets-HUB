using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface IIdentityService
{
    Task<List<UserDto>> GetAllUsersAsync(string bearerToken);
    Task<OperationResult> ToggleUserStatusAsync(int userId, string bearerToken);
}
