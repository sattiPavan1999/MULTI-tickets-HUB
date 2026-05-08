using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public class AuthService(
    IAuthenticationService authenticationService,
    IUserAccountService userAccountService,
    IPasswordService passwordService) : IAuthService
{
    public Task<UserType> RegisterAsync(RegisterInput input, CancellationToken ct = default)
        => authenticationService.RegisterAsync(input, ct);

    public Task<LoginResponse> LoginAsync(LoginInput input, CancellationToken ct = default)
        => authenticationService.LoginAsync(input, ct);

    public Task<UserType?> GetUserByIdAsync(int id, CancellationToken ct = default)
        => userAccountService.GetUserByIdAsync(id, ct);

    public Task<UserType> UpdateProfileAsync(int userId, UpdateProfileInput input, CancellationToken ct = default)
        => userAccountService.UpdateProfileAsync(userId, input, ct);

    public Task<List<UserType>> GetAllUsersAsync(CancellationToken ct = default)
        => userAccountService.GetAllUsersAsync(ct);

    public Task<int> GetUserCountAsync(CancellationToken ct = default)
        => userAccountService.GetUserCountAsync(ct);

    public Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordInput input, CancellationToken ct = default)
        => passwordService.ForgotPasswordAsync(input, ct);

    public Task<OperationResult> ResetPasswordAsync(ResetPasswordInput input, CancellationToken ct = default)
        => passwordService.ResetPasswordAsync(input, ct);

    public Task<OperationResult> ToggleUserStatusAsync(int userId, CancellationToken ct = default)
        => userAccountService.ToggleUserStatusAsync(userId, ct);
}
