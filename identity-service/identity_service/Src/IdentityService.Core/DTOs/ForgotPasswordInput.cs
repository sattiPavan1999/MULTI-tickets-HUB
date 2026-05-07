using System.ComponentModel.DataAnnotations;

namespace IdentityService.Core.DTOs;

/// <summary>
/// Request to start a "Forgot Password" flow.
/// </summary>
public class ForgotPasswordInput
{
    /// <summary>
    /// Email address of the account that needs a reset.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; set; }
}
