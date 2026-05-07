using System.ComponentModel.DataAnnotations;

namespace IdentityService.Core.DTOs;

/// <summary>
/// Request to complete a "Forgot Password" flow with a new password.
/// </summary>
public class ResetPasswordInput
{
    /// <summary>
    /// Plain reset token issued by ForgotPassword.
    /// </summary>
    [Required(ErrorMessage = "Reset token is required")]
    public required string Token { get; set; }

    /// <summary>
    /// New password to set on the account.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public required string NewPassword { get; set; }
}
