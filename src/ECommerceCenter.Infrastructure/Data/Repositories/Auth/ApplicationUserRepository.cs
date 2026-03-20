using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Auth;

public class ApplicationUserRepository(AppDbContext context)
    : GenericRepository<ApplicationUser>(context), IApplicationUserRepository
{
    public async Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await Context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<ApplicationUser?> GetByEmailWithRefreshTokensAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await Context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<ApplicationUser?> GetByEmailWithPersonAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await Context.Users
            .Include(u => u.Person)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<ApplicationUser?> GetByIdWithPersonAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await Context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
}

