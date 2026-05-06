using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models.DTOs;

/// <summary>
/// Profile update request input
/// </summary>
public class UpdateProfileInput
{
    /// <summary>
    /// Updated full name (optional)
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Updated phone number (optional)
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
}
