
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository(AppDbContext context) : IPasswordResetTokenRepository
{
    public async Task AddAsync(PasswordResetToken token)
    {
        await context.PasswordResetTokens.AddAsync(token);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string token)
    {
        var tokenEntity = await context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (tokenEntity != null)
        {
            context.PasswordResetTokens.Remove(tokenEntity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token);
    }
}
