
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token)
    {
        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string token)
    {
        var tokenEntity = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (tokenEntity != null)
        {
            context.RefreshTokens.Remove(tokenEntity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        context.RefreshTokens.Update(token);
        await context.SaveChangesAsync();
    }
}
