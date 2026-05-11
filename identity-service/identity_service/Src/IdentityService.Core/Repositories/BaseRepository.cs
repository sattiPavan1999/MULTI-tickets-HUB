using IdentityService.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Core.Repositories;

public abstract class BaseRepository<T>(IdentityDbContext context) : IBaseRepository<T>
    where T : class
{
    protected readonly IdentityDbContext _context = context;

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _context.Set<T>().FindAsync([id]);

    public virtual async Task<T> AddAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual IQueryable<T> Query()
        => _context.Set<T>().AsNoTracking();
}
