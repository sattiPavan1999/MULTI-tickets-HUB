namespace IdentityService.Core.DTOs;

/// <summary>
/// Response from a "Forgot Password" request.
/// To avoid account enumeration the response is identical whether or not the email exists.
/// In this simulated implementation, when the email matches a real user the plain reset token
/// is returned in <see cref="ResetToken"/>; in production it would be delivered by email and
/// this field would be null.
/// </summary>
public class ForgotPasswordResponse
{
    public required string Message { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
