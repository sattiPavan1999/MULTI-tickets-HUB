using IdentityService.Models.DTOs;
using IdentityService.Models.GraphQL;

namespace IdentityService.Services;

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
}
