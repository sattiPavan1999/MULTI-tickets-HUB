using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public interface IPasswordService
{
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordInput input);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordInput input);
}
