using IdentityService.Core.Models;

namespace IdentityService.Core.Repositories;

/// <summary>
/// Repository for password reset tokens.
/// </summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// Persist a new reset token.
    /// </summary>
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);

    /// <summary>
    /// Find an unused, unexpired token by its hash.
    /// </summary>
    Task<PasswordResetToken?> GetActiveByHashAsync(string tokenHash);

    /// <summary>
    /// Mark a token as used.
    /// </summary>
    Task MarkUsedAsync(PasswordResetToken token);

    /// <summary>
    /// Mark every active token for the user as used (used when issuing a fresh one).
    /// </summary>
    Task InvalidateActiveForUserAsync(int userId);
}
