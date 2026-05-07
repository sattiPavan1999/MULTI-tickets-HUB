using IdentityService.Core.DTOs;

namespace IdentityService.Core.DTOs;

/// <summary>
/// Login response containing JWT token and user information
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT authentication token
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public required UserType User { get; set; }
}
