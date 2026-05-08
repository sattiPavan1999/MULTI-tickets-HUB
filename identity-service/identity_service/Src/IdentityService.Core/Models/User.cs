namespace IdentityService.Core.Models;

/// <summary>
/// User entity representing identity information
/// </summary>
public class User
{
    /// <summary>
    /// Primary key, unique user identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Hashed password (never stored plain)
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// User's contact phone number
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// User role: User or Admin
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the account is active; deactivated users cannot log in
    /// </summary>
    public bool IsActive { get; set; } = true;
}
