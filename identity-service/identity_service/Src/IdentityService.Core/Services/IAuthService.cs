using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public interface IAuthService
{
    Task<UserType> RegisterAsync(RegisterInput input, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(LoginInput input, CancellationToken ct = default);
}
