using IdentityService.Data;
using IdentityService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IdentityDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User created with ID: {UserId}", user.Id);

        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User updated: {UserId}", user.Id);

        return user;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.AsNoTracking().ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Users.CountAsync();
    }
}
