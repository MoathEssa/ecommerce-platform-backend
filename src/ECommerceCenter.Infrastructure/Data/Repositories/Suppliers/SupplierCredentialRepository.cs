using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Suppliers;
using ECommerceCenter.Domain.Entities.Suppliers;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Suppliers;

public class SupplierCredentialRepository(AppDbContext context) : ISupplierCredentialRepository
{
    public async Task<SupplierCredential?> GetByTypeAsync(
        SupplierType supplierType,
        CancellationToken cancellationToken = default)
        => await context.SupplierCredentials
            .FirstOrDefaultAsync(c => c.SupplierType == supplierType, cancellationToken);

    public async Task UpsertAsync(
        SupplierCredential credential,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.SupplierCredentials
            .FirstOrDefaultAsync(c => c.SupplierType == credential.SupplierType, cancellationToken);

        if (existing is null)
        {
            await context.SupplierCredentials.AddAsync(credential, cancellationToken);
        }
        else
        {
            existing.ApiKey                  = credential.ApiKey;
            existing.OpenId                  = credential.OpenId;
            existing.AccessToken             = credential.AccessToken;
            existing.AccessTokenExpiryDate   = credential.AccessTokenExpiryDate;
            existing.RefreshToken            = credential.RefreshToken;
            existing.RefreshTokenExpiryDate  = credential.RefreshTokenExpiryDate;
            existing.IsActive                = credential.IsActive;
            existing.LastRefreshedAt         = credential.LastRefreshedAt;
            context.SupplierCredentials.Update(existing);
        }
    }

    public async Task<IReadOnlyList<SupplierCredential>> GetAllExpiringSoonAsync(
        int withinHours,
        CancellationToken cancellationToken = default)
    {
        var threshold = DateTimeOffset.UtcNow.AddHours(withinHours);
        return await context.SupplierCredentials
            .Where(c => c.IsActive
                        && c.AccessToken != null
                        && c.AccessTokenExpiryDate <= threshold)
            .ToListAsync(cancellationToken);
    }
}
