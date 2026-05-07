using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IdentityService.Core.DTOs;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;

namespace IdentityService.Core.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _resetTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository resetTokenRepository,
        IJwtService jwtService,
        IAuditService auditService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _resetTokenRepository = resetTokenRepository;
        _jwtService = jwtService;
        _auditService = auditService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UserType> RegisterAsync(RegisterInput input)
    {
        if (await _userRepository.EmailExistsAsync(input.Email))
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", input.Email);
            throw new InvalidOperationException("Email already registered");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(input.Password);

        var user = new User
        {
            Email = input.Email,
            PasswordHash = passwordHash,
            FullName = input.FullName,
            PhoneNumber = input.PhoneNumber,
            Role = "User"
        };

        var createdUser = await _userRepository.CreateAsync(user);

        await _auditService.LogAsync($"User registered: {createdUser.Email}");

        return MapToUserType(createdUser);
    }

    public async Task<LoginResponse> LoginAsync(LoginInput input)
    {
        var user = await _userRepository.GetByEmailAsync(input.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", input.Email);
            await _auditService.LogAsync($"Failed login attempt for: {input.Email}");
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var token = _jwtService.GenerateToken(user);

        await _auditService.LogAsync($"User logged in: {user.Email}");

        return new LoginResponse
        {
            Token = token,
            User = MapToUserType(user)
        };
    }

    public async Task<UserType?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            return null;
        }

        return MapToUserType(user);
    }

    public async Task<UserType> UpdateProfileAsync(int userId, UpdateProfileInput input)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!string.IsNullOrWhiteSpace(input.FullName))
        {
            user.FullName = input.FullName;
        }

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            user.PhoneNumber = input.PhoneNumber;
        }

        if (!string.IsNullOrWhiteSpace(input.Email)
            && !string.Equals(input.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _userRepository.EmailExistsAsync(input.Email))
            {
                _logger.LogWarning("Profile update rejected — email already registered: {Email}", input.Email);
                throw new InvalidOperationException("Email already registered");
            }

            user.Email = input.Email;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);

        await _auditService.LogAsync($"User profile updated: {updatedUser.Email}");

        return MapToUserType(updatedUser);
    }

    public async Task<List<UserType>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserType).ToList();
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _userRepository.CountAsync();
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordInput input)
    {
        var user = await _userRepository.GetByEmailAsync(input.Email);

        if (user == null)
        {
            _logger.LogInformation("Forgot password requested for unknown email: {Email}", input.Email);
            await _auditService.LogAsync($"Forgot password (no user): {input.Email}");
            return new ForgotPasswordResponse
            {
                Message = "If the email is registered, a reset token has been issued."
            };
        }

        await _resetTokenRepository.InvalidateActiveForUserAsync(user.Id);

        var plainToken = GenerateResetToken();
        var tokenHash = HashToken(plainToken);
        var expiryMinutes = int.Parse(_configuration["PasswordReset:TokenExpiryMinutes"] ?? "30");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        await _resetTokenRepository.CreateAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        });

        await _auditService.LogAsync($"Password reset token issued for: {user.Email}");

        return new ForgotPasswordResponse
        {
            Message = "If the email is registered, a reset token has been issued.",
            ResetToken = plainToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordInput input)
    {
        var tokenHash = HashToken(input.Token);
        var resetToken = await _resetTokenRepository.GetActiveByHashAsync(tokenHash);

        if (resetToken == null)
        {
            _logger.LogWarning("Password reset attempted with invalid or expired token");
            await _auditService.LogAsync("Password reset failed: invalid or expired token");
            throw new UnauthorizedAccessException("Reset token is invalid or has expired");
        }

        var user = await _userRepository.GetByIdAsync(resetToken.UserId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
        await _userRepository.UpdateAsync(user);

        await _resetTokenRepository.MarkUsedAsync(resetToken);

        await _auditService.LogAsync($"Password reset for user: {user.Email}");

        return new OperationResult
        {
            Success = true,
            Message = "Password has been reset"
        };
    }

    private static string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string HashToken(string plainToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static UserType MapToUserType(User user)
    {
        return new UserType
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
