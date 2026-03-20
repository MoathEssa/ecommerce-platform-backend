using ECommerceCenter.Domain.Entities.Auth;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;

public interface IApplicationUserRepository : IGenericRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailWithRefreshTokensAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailWithPersonAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByIdWithPersonAsync(int id, CancellationToken cancellationToken = default);
}
