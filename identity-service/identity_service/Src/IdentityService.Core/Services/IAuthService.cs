using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public interface IAuthService
{
    Task<UserType> RegisterAsync(RegisterInput input, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(LoginInput input, CancellationToken ct = default);
    Task<UserType?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<UserType> UpdateProfileAsync(int userId, UpdateProfileInput input, CancellationToken ct = default);
    Task<List<UserType>> GetAllUsersAsync(CancellationToken ct = default);
    Task<int> GetUserCountAsync(CancellationToken ct = default);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordInput input, CancellationToken ct = default);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordInput input, CancellationToken ct = default);
}
