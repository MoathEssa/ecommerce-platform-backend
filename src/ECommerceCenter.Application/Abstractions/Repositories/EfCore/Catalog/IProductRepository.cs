using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken cancellationToken = default);

    /// <summary>Returns the next value from the slug-suffix DB sequence (monotonically increasing, collision-free).</summary>
    Task<long> NextSlugSuffixAsync(CancellationToken cancellationToken = default);

    // ── Read projections ───────────────────────────────────────────────────

    /// <summary>Filtered, sorted, paged storefront listing. Returns flat projection rows — no DTOs.</summary>
    Task<(List<ProductListingRow> Items, int TotalCount)> GetProductListingPagedAsync(
        ProductListFilter filter,
        IReadOnlyCollection<int>? categoryIds,
        CancellationToken ct = default);

    /// <summary>Full storefront product detail projected into row types. Returns null when not found/inactive.</summary>
    Task<ProductDetailRow?> GetProductDetailBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Returns title+slug rows for search-suggestion auto-complete.</summary>
    Task<List<CatalogSearchRow>> SearchByTitleAsync(string term, int limit, CancellationToken ct = default);

    /// <summary>Filtered, sorted, paged admin product listing.</summary>
    Task<(List<AdminProductListingRow> Items, int TotalCount)> GetAdminProductListingPagedAsync(
        AdminProductListFilter filter, CancellationToken ct = default);

    /// <summary>Full admin product detail projected into row types. Returns null when not found.</summary>
    Task<AdminProductDetailRow?> GetAdminProductDetailByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Returns the CJ-sourced product with its Variants and Images collections loaded
    /// and <em>tracked</em> by EF Core, so callers can add new variants without an
    /// additional round-trip. Returns <c>null</c> when no matching product exists.
    /// </summary>
    Task<Product?> FindCjProductWithVariantsAsync(
        string cjProductId, CancellationToken ct = default);
}
