using Microsoft.EntityFrameworkCore;

namespace TrainService.Core.Repositories;

public abstract class BaseRepository<T>(DbContext context) : IBaseRepository<T> where T : class
{
    protected readonly DbContext Context = context;

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Context.Set<T>().FindAsync([id], ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await Context.Set<T>().AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
        {
            Context.Set<T>().Remove(entity);
            await Context.SaveChangesAsync(ct);
        }
    }

    public IQueryable<T> Query() => Context.Set<T>().AsQueryable();
}
