using ECommerceCenter.Domain.Entities.Inventory;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;

public interface IInventoryItemRepository : IGenericRepository<InventoryItem>
{
    /// <summary>Loads the item with its RowVersion for optimistic concurrency checks.</summary>
    Task<InventoryItem?> GetByVariantIdAsync(int variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-loads inventory items (with RowVersion) for the given variant ids.
    /// Returns a dictionary keyed by VariantId — items not found are absent from the result.
    /// Replaces per-item calls in the checkout hot path.
    /// </summary>
    Task<Dictionary<int, InventoryItem>> GetByVariantIdsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default);

    /// <summary>Filtered, sorted, paged list with Variant→Product navigation for the admin inventory view.</summary>
    Task<(List<InventoryItem> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? stockStatus, string sortBy,
        CancellationToken cancellationToken = default);

    /// <summary>Single item with Variant→Product navigation for the admin inventory detail view.</summary>
    Task<InventoryItem?> GetWithNavigationByVariantIdAsync(
        int variantId, CancellationToken cancellationToken = default);
}
