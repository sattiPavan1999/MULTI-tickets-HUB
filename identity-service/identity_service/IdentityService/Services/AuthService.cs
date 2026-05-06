using IdentityService.Models.DTOs;
using IdentityService.Models.Entities;
using IdentityService.Models.GraphQL;
using IdentityService.Repositories;

namespace IdentityService.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IAuditService auditService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _auditService = auditService;
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
