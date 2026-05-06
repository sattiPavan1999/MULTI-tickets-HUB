using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.DTOs;

/// <summary>
/// Registration request input
/// </summary>
public class RegisterInput
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; set; }

    /// <summary>
    /// User's password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public required string Password { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    [Required(ErrorMessage = "Full name is required")]
    public required string FullName { get; set; }

    /// <summary>
    /// User's contact phone number
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public required string PhoneNumber { get; set; }
}
