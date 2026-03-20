using ECommerceCenter.Domain.Entities.Inventory;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;

public interface IInventoryAdjustmentRepository : IGenericRepository<InventoryAdjustment>
{
    Task<IEnumerable<InventoryAdjustment>> GetByVariantIdPagedAsync(
        int variantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountByVariantIdAsync(int variantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent <paramref name="take"/> adjustments for a variant, newest first.</summary>
    Task<List<InventoryAdjustment>> GetRecentByVariantIdAsync(
        int variantId, int take, CancellationToken cancellationToken = default);
}
