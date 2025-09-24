
using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByProviderAsync(string provider, string providerUserId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
