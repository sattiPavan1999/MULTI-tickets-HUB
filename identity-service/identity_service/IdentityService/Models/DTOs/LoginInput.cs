using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.DTOs;

/// <summary>
/// Login request input
/// </summary>
public class LoginInput
{
    /// <summary>
    /// User's registered email
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; set; }

    /// <summary>
    /// User's password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}
