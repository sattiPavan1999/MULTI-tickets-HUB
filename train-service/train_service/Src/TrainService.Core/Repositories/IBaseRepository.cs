namespace TrainService.Core.Repositories;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
    IQueryable<T> Query();
}
