using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public interface IUserAccountService
{
    Task<UserType?> GetUserByIdAsync(int id);
    Task<UserType> UpdateProfileAsync(int userId, UpdateProfileInput input);
    Task<List<UserType>> GetAllUsersAsync();
    Task<int> GetUserCountAsync();
    Task<OperationResult> ToggleUserStatusAsync(int userId);
}
