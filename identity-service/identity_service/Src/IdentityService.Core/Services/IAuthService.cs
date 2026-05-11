using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public interface IAuthService
{
    Task<UserType> RegisterAsync(RegisterInput input);
    Task<LoginResponse> LoginAsync(LoginInput input);
}
