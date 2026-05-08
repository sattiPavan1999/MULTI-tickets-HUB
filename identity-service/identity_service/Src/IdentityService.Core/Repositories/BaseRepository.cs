using IdentityService.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Core.Repositories;

public abstract class BaseRepository<T>(IdentityDbContext context) : IBaseRepository<T>
    where T : class
{
    protected readonly IdentityDbContext _context = context;

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Set<T>().FindAsync([id], ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public virtual IQueryable<T> Query()
        => _context.Set<T>().AsNoTracking();
}
