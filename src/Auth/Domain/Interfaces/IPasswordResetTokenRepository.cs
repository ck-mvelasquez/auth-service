using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task AddAsync(PasswordResetToken token);
        Task DeleteAsync(string token);
    }
}
