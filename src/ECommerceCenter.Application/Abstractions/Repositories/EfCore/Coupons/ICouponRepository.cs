using ECommerceCenter.Domain.Entities.Coupons;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;

public interface ICouponRepository : IGenericRepository<Coupon>
{
    /// <summary>
    /// Loads the coupon with all three applicability id-collections (AsNoTracking).
    /// Used by <c>CouponEvaluator</c> to determine discount eligibility.
    /// </summary>
    Task<Coupon?> GetByCodeWithRulesAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the subset of <paramref name="productIds"/> that belong to at least one
    /// of the given <paramref name="categoryIds"/>.
    /// AsNoTracking projection — single SQL query.
    /// </summary>
    Task<IReadOnlySet<int>> GetProductIdsInCategoriesAsync(
        IReadOnlyCollection<int> categoryIds,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    /// <summary>Returns how many times the given user has already used this coupon.</summary>
    Task<int> GetUsageCountByUserAsync(int couponId, int userId, CancellationToken cancellationToken = default);

    /// <summary>Returns how many times this coupon has been used globally (cross-checks UsedCount).</summary>
    Task<int> GetTotalUsageCountAsync(int couponId, CancellationToken cancellationToken = default);

    /// <summary>Loads the coupon by ID with tracking and all applicability collections included.</summary>
    Task<Coupon?> GetByIdWithRulesTrackedAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces all applicability rules (categories, products, variants) in a single operation.
    /// Deletes existing rules and inserts new ones.
    /// </summary>
    Task ReplaceApplicabilityRulesAsync(
        int couponId,
        int[] categoryIds, int[] productIds, int[] variantIds,
        CancellationToken cancellationToken = default);

    /// <summary>Filtered, sorted, paged list for the admin coupon view.</summary>
    Task<(List<Coupon> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, bool? isActive, string sortBy,
        CancellationToken cancellationToken = default);

    /// <summary>Loads a coupon with its three applicability collections (AsNoTracking) for the detail view.</summary>
    Task<Coupon?> GetByIdWithApplicabilityAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Returns aggregate usage stats for a coupon.</summary>
    Task<(int TotalUsed, int UniqueUsers, decimal TotalDiscountGiven)> GetUsageStatsAsync(
        int couponId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent <paramref name="take"/> usages with Order navigation loaded.</summary>
    Task<List<CouponUsage>> GetRecentUsageDataAsync(
        int couponId, int take, CancellationToken cancellationToken = default);

    /// <summary>Paged usages with Order navigation loaded for the admin usage list.</summary>
    Task<(List<CouponUsage> Items, int TotalCount)> GetUsagesPagedAsync(
        int couponId, int page, int pageSize, CancellationToken cancellationToken = default);
}
