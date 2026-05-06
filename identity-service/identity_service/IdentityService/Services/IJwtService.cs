using IdentityService.Models.Entities;

namespace IdentityService.Services;

/// <summary>
/// JWT token service interface
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    bool ValidateToken(string token);
}
