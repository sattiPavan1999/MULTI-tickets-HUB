using AdminBFF.Core.DTOs;

namespace AdminBFF.Core.Services;

public interface IIdentityService
{
    Task<UserDto> GetUserByIdAsync(int userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<int> GetActiveUserCountAsync();
    Task<OperationResultDto> DeactivateUserAsync(int userId);
}
