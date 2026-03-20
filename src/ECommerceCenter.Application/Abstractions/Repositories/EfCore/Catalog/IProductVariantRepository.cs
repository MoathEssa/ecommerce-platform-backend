using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

public interface IProductVariantRepository : IGenericRepository<ProductVariant>
{
    Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    // ── Admin ──────────────────────────────────────────────────────────────
    Task<ProductVariant?> GetByIdAndProductAsync(int variantId, int productId, CancellationToken cancellationToken = default);

    Task<bool> SkuExistsAsync(string sku, int? excludeId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveVariantsAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>Returns the next value from the SKU DB sequence, used to auto-generate SKU = "SKU-{n:D6}".</summary>
    Task<long> NextSkuAsync(CancellationToken cancellationToken = default);

    /// <summary>Loads a variant with its parent Product — used by cart item validation.</summary>
    Task<ProductVariant?> GetWithProductAsync(int variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-loads variants with their parent Products for the given variant ids.
    /// Returns a dictionary keyed by VariantId — items not found are absent from the result.
    /// Replaces per-item calls in the checkout hot path.
    /// </summary>
    Task<Dictionary<int, ProductVariant>> GetWithProductsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a variantId → productId map for the given variant ids.
    /// AsNoTracking projection — single SQL query, no entity graph.
    /// </summary>
    Task<Dictionary<int, int>> GetProductIdsByVariantIdsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default);

    /// <summary>Projected variant detail for the storefront variant-detail endpoint. Returns null when not found/inactive.</summary>
    Task<VariantDetailRow?> GetVariantDetailAsync(
        string productSlug, int variantId, CancellationToken ct = default);
}
