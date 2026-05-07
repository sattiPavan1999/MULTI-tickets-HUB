using System.ComponentModel.DataAnnotations;

namespace IdentityService.Core.DTOs;

/// <summary>
/// Profile update request input
/// </summary>
public class UpdateProfileInput
{
    /// <summary>
    /// Updated full name (optional)
    /// </summary>
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Full name must be between 1 and 255 characters")]
    public string? FullName { get; set; }

    /// <summary>
    /// Updated phone number (optional)
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 20 characters")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Updated email address (optional)
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }
}
