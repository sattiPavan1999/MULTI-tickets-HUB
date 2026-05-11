using AutoMapper;
using FluentValidation;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Models;
using IdentityService.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace IdentityService.Core.Services;

public class AuthService(
    IUserRepository userRepository,
    IJwtService jwtService,
    IAuditService auditService,
    IValidator<RegisterInput> registerValidator,
    IValidator<LoginInput> loginValidator,
    IMapper mapper,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<UserType> RegisterAsync(RegisterInput input)
    {
        await registerValidator.ValidateAndThrowAsync(input);

        if (await userRepository.EmailExistsAsync(input.Email))
        {
            logger.LogWarning("Registration attempt with existing email: {Email}", input.Email);
            throw new ConflictException("Email already registered");
        }

        var user = new User
        {
            Email = input.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
            FullName = input.FullName,
            PhoneNumber = input.PhoneNumber,
            Role = Roles.User
        };

        var created = await userRepository.AddAsync(user);
        await auditService.LogAsync($"User registered: {created.Email}");
        return mapper.Map<UserType>(created);
    }

    public async Task<LoginResponse> LoginAsync(LoginInput input)
    {
        await loginValidator.ValidateAndThrowAsync(input);

        var user = await userRepository.GetByEmailAsync(input.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for email: {Email}", input.Email);
            await auditService.LogAsync($"Failed login attempt for: {input.Email}");
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Login attempt on deactivated account: {Email}", input.Email);
            await auditService.LogAsync($"Login blocked — account deactivated: {input.Email}");
            throw new UnauthorizedAccessException("Account is deactivated");
        }

        var token = jwtService.GenerateToken(user);
        await auditService.LogAsync($"User logged in: {user.Email}");

        return new LoginResponse { Token = token, User = mapper.Map<UserType>(user) };
    }
}
