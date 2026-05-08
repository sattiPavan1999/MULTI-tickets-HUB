using IdentityService.Core.Data;
using IdentityService.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Core.Repositories;

public class UserRepository(IdentityDbContext context, ILogger<UserRepository> logger)
    : BaseRepository<User>(context), IUserRepository
{
    private readonly ILogger<UserRepository> _logger = logger;

    public override async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("User created with ID: {UserId}", user.Id);
        return user;
    }

    public override async Task<User> UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("User updated: {UserId}", user.Id);
        return user;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _context.Users.AnyAsync(u => u.Email == email, ct);

    public Task<List<User>> GetAllAsync(CancellationToken ct = default)
        => _context.Users.AsNoTracking().ToListAsync(ct);

    public Task<int> CountAsync(CancellationToken ct = default)
        => _context.Users.CountAsync(ct);
}
