using ECommerceCenter.Domain.Entities.Auth;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetValidTokenWithUserAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
    void RevokeToken(RefreshToken token, string reason);
    Task RevokeAllUserTokensAsync(int userId, string reason, CancellationToken cancellationToken = default);
}
