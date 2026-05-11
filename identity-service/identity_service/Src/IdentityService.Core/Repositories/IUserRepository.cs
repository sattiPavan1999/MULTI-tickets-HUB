using IdentityService.Core.Models;

namespace IdentityService.Core.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<List<User>> GetAllAsync();
    Task<int> CountAsync();
}
