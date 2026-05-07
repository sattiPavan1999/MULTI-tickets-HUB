using IdentityService.Core.Models;

namespace IdentityService.Core.Repositories;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Create new user
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Update existing user
    /// </summary>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Check if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    Task<List<User>> GetAllAsync();

    Task<int> CountAsync();
}
