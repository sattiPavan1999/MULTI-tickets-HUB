namespace IdentityService.Core.DTOs;

public class RegisterInput
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
}
