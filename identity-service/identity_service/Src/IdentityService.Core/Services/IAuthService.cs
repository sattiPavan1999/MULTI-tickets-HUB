using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register new user
    /// </summary>
    Task<UserType> RegisterAsync(RegisterInput input);

    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginInput input);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserType?> GetUserByIdAsync(int id);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<UserType> UpdateProfileAsync(int userId, UpdateProfileInput input);

    Task<List<UserType>> GetAllUsersAsync();

    Task<int> GetUserCountAsync();

    /// <summary>
    /// Initiate a "Forgot Password" flow. Returns a response that always succeeds
    /// to avoid leaking which emails are registered.
    /// </summary>
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordInput input);

    /// <summary>
    /// Complete a "Forgot Password" flow by consuming a reset token and setting a new password.
    /// </summary>
    Task<OperationResult> ResetPasswordAsync(ResetPasswordInput input);
}
