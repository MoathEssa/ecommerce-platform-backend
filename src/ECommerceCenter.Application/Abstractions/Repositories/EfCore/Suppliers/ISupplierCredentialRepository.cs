using ECommerceCenter.Domain.Entities.Suppliers;
using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Suppliers;

public interface ISupplierCredentialRepository
{
    Task<SupplierCredential?> GetByTypeAsync(
        SupplierType supplierType,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        SupplierCredential credential,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all active credentials whose access token expires within <paramref name="withinHours"/> hours.</summary>
    Task<IReadOnlyList<SupplierCredential>> GetAllExpiringSoonAsync(
        int withinHours,
        CancellationToken cancellationToken = default);
}
