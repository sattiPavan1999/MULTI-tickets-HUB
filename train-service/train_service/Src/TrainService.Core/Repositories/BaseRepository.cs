using Microsoft.EntityFrameworkCore;

namespace TrainService.Core.Repositories;

public abstract class BaseRepository<T>(DbContext context) : IBaseRepository<T> where T : class
{
    protected readonly DbContext Context = context;

    public async Task<T?> GetByIdAsync(int id)
        => await Context.Set<T>().FindAsync([id]);

    public async Task<T> AddAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            Context.Set<T>().Remove(entity);
            await Context.SaveChangesAsync();
        }
    }

    public IQueryable<T> Query() => Context.Set<T>().AsQueryable();
}
