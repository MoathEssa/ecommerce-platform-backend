using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> HasProductAssignmentsAsync(int id, CancellationToken cancellationToken = default);

    // ── Read projections ───────────────────────────────────────────────────

    /// <summary>All active categories ordered by SortOrder — used for tree building, breadcrumbs, and storefront validation.</summary>
    Task<List<Category>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>All categories including inactive — used by admin depth/ancestor checks.</summary>
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns title+slug rows for search-suggestion auto-complete (active categories only).</summary>
    Task<List<CatalogSearchRow>> SearchByNameAsync(string term, int limit, CancellationToken ct = default);

    /// <summary>
    /// Returns true if another category with the same parentId already uses the given sortOrder.
    /// Pass excludeId to ignore the current category when updating.
    /// </summary>
    Task<bool> SortOrderExistsAmongSiblingsAsync(int? parentId, int sortOrder, int? excludeId, CancellationToken ct = default);

    /// <summary>Draws the next value from the shared <c>slug_suffix_seq</c> DB sequence — guarantees uniqueness without loops.</summary>
    Task<long> NextSlugSuffixAsync(CancellationToken ct = default);
}
