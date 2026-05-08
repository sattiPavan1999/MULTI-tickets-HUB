using IdentityService.Core.DTOs;

namespace IdentityService.Core.Services;

public interface IPasswordService
{
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordInput input, CancellationToken ct = default);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordInput input, CancellationToken ct = default);
}
