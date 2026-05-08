using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public interface IUserAccountService
{
    Task<UserType?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<UserType> UpdateProfileAsync(int userId, UpdateProfileInput input, CancellationToken ct = default);
    Task<List<UserType>> GetAllUsersAsync(CancellationToken ct = default);
    Task<int> GetUserCountAsync(CancellationToken ct = default);
}
