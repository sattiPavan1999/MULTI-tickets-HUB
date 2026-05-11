using System.Security.Cryptography;
using System.Text;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityService.Core.Services;

public class PasswordService(
    IUserRepository userRepository,
    IPasswordResetTokenRepository resetTokenRepository,
    IAuditService auditService,
    IConfiguration configuration,
    ILogger<PasswordService> logger) : IPasswordService
{
    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordInput input)
    {
        var user = await userRepository.GetByEmailAsync(input.Email);

        if (user is null)
        {
            logger.LogInformation("Forgot password requested for unknown email: {Email}", input.Email);
            await auditService.LogAsync($"Forgot password (no user): {input.Email}");
            return new ForgotPasswordResponse { Message = "If the email is registered, a reset token has been issued." };
        }

        await resetTokenRepository.InvalidateActiveForUserAsync(user.Id);

        var plainToken = GenerateResetToken();
        var expiryMinutes = int.Parse(configuration["PasswordReset:TokenExpiryMinutes"] ?? "30");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        await resetTokenRepository.CreateAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(plainToken),
            ExpiresAt = expiresAt
        });

        await auditService.LogAsync($"Password reset token issued for: {user.Email}");

        var isDevelopment = string.Equals(
            configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase);

        return new ForgotPasswordResponse
        {
            Message = "If the email is registered, a reset token has been issued.",
            ResetToken = isDevelopment ? plainToken : null,
            ExpiresAt = isDevelopment ? expiresAt : null
        };
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordInput input)
    {
        var resetToken = await resetTokenRepository.GetActiveByHashAsync(HashToken(input.Token));

        if (resetToken is null)
        {
            logger.LogWarning("Password reset attempted with invalid or expired token");
            await auditService.LogAsync("Password reset failed: invalid or expired token");
            throw new UnauthorizedAccessException("Reset token is invalid or has expired");
        }

        var user = await userRepository.GetByIdAsync(resetToken.UserId)
            ?? throw new NotFoundException("User not found");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
        await userRepository.UpdateAsync(user);
        await resetTokenRepository.MarkUsedAsync(resetToken);
        await auditService.LogAsync($"Password reset for user: {user.Email}");

        return new OperationResult { Success = true, Message = "Password has been reset" };
    }

    private static string GenerateResetToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private static string HashToken(string plainToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plainToken))).ToLowerInvariant();
}
