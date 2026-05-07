using IdentityService.Core.Data;
using IdentityService.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Core.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<PasswordResetTokenRepository> _logger;

    public PasswordResetTokenRepository(
        IdentityDbContext context,
        ILogger<PasswordResetTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
    {
        token.CreatedAt = DateTime.UtcNow;
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Password reset token issued for user: {UserId}", token.UserId);

        return token;
    }

    public async Task<PasswordResetToken?> GetActiveByHashAsync(string tokenHash)
    {
        var now = DateTime.UtcNow;
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t =>
                t.TokenHash == tokenHash
                && t.UsedAt == null
                && t.ExpiresAt > now);
    }

    public async Task MarkUsedAsync(PasswordResetToken token)
    {
        token.UsedAt = DateTime.UtcNow;
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidateActiveForUserAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var active = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > now)
            .ToListAsync();

        foreach (var token in active)
        {
            token.UsedAt = now;
        }

        if (active.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }
}
