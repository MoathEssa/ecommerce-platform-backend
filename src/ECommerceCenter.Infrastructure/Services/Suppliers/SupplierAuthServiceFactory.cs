using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Infrastructure.Suppliers;

/// <summary>
/// Resolves the correct <see cref="ISupplierAuthService"/> for a given <see cref="SupplierType"/>.
/// New suppliers are automatically discovered when their implementation is registered in DI —
/// no changes to this class are ever needed (Open/Closed Principle).
/// </summary>
public sealed class SupplierAuthServiceFactory(IEnumerable<ISupplierAuthService> services)
{
    private readonly IReadOnlyDictionary<SupplierType, ISupplierAuthService> _map =
        services.ToDictionary(s => s.SupplierType);

    /// <summary>Returns the service for the requested supplier.</summary>
    /// <exception cref="NotSupportedException">Thrown when no implementation is registered for the supplier.</exception>
    public ISupplierAuthService GetService(SupplierType supplierType)
    {
        if (_map.TryGetValue(supplierType, out var service))
            return service;

        throw new NotSupportedException(
            $"No ISupplierAuthService implementation is registered for supplier '{supplierType}'.");
    }
}
