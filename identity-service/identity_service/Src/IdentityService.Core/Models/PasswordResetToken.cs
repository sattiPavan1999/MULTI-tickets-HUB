namespace IdentityService.Core.Models;

/// <summary>
/// One-time token issued for the "Forgot Password" flow.
/// Only the SHA-256 hash of the token is stored; the plain token is delivered to the user once.
/// </summary>
public class PasswordResetToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
