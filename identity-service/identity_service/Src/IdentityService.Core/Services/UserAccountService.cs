using AutoMapper;
using FluentValidation;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace IdentityService.Core.Services;

public class UserAccountService(
    IUserRepository userRepository,
    IAuditService auditService,
    IValidator<UpdateProfileInput> updateProfileValidator,
    IMapper mapper,
    ILogger<UserAccountService> logger) : IUserAccountService
{
    public async Task<UserType?> GetUserByIdAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        return user is null ? null : mapper.Map<UserType>(user);
    }

    public async Task<UserType> UpdateProfileAsync(int userId, UpdateProfileInput input)
    {
        await updateProfileValidator.ValidateAndThrowAsync(input);

        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(input.FullName))
            user.FullName = input.FullName;

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
            user.PhoneNumber = input.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(input.Email)
            && !string.Equals(input.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await userRepository.EmailExistsAsync(input.Email))
            {
                logger.LogWarning("Profile update rejected — email already registered: {Email}", input.Email);
                throw new ConflictException("Email already registered");
            }

            user.Email = input.Email;
        }

        var updated = await userRepository.UpdateAsync(user);
        await auditService.LogAsync($"User profile updated: {updated.Email}");
        return mapper.Map<UserType>(updated);
    }

    public async Task<List<UserType>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return mapper.Map<List<UserType>>(users);
    }

    public Task<int> GetUserCountAsync()
        => userRepository.CountAsync();

    public async Task<OperationResult> ToggleUserStatusAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        user.IsActive = !user.IsActive;
        await userRepository.UpdateAsync(user);
        await auditService.LogAsync($"User status toggled: {user.Email} → IsActive={user.IsActive}");

        var status = user.IsActive ? "activated" : "deactivated";
        return new OperationResult { Success = true, Message = $"User account {status}" };
    }
}
