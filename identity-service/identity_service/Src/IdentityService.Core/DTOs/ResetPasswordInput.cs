namespace IdentityService.Core.DTOs;

public class ResetPasswordInput
{
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}
