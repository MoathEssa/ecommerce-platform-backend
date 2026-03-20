using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Auth;

public class RefreshTokenRepository(AppDbContext context)
    : GenericRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetValidTokenWithUserAsync(
        string token,
        CancellationToken cancellationToken = default)
        => await Context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(
                rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    public async Task<RefreshToken?> GetValidTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
        => await Context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.Token == token && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    public void RevokeToken(RefreshToken token, string reason)
    {
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = reason;
        Context.RefreshTokens.Update(token);
    }

    public async Task RevokeAllUserTokensAsync(
        int userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await Context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
        }

        Context.RefreshTokens.UpdateRange(activeTokens);
    }
}
