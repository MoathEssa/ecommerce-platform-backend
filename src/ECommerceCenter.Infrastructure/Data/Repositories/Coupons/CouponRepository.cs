using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Entities.Coupons;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Coupons;

public class CouponRepository(AppDbContext context)
    : GenericRepository<Coupon>(context), ICouponRepository
{
    public async Task<Coupon?> GetByCodeWithRulesAsync(
        string code,
        CancellationToken cancellationToken = default)
        => await Context.Set<Coupon>()
            .AsNoTracking()
            .Include(c => c.ApplicableCategories)
            .Include(c => c.ApplicableProducts)
            .Include(c => c.ApplicableVariants)
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

    public async Task<IReadOnlySet<int>> GetProductIdsInCategoriesAsync(
        IReadOnlyCollection<int> categoryIds,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        var result = await Context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId.HasValue &&
                        categoryIds.Contains(p.CategoryId.Value) &&
                        productIds.Contains(p.Id))
            .Select(p => p.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        return result.ToHashSet();
    }

    public async Task<int> GetUsageCountByUserAsync(
        int couponId,
        int userId,
        CancellationToken cancellationToken = default)
        => await Context.Set<CouponUsage>()
            .CountAsync(u => u.CouponId == couponId && u.UserId == userId, cancellationToken);

    public async Task<int> GetTotalUsageCountAsync(
        int couponId,
        CancellationToken cancellationToken = default)
        => await Context.Set<CouponUsage>()
            .CountAsync(u => u.CouponId == couponId, cancellationToken);

    public async Task<Coupon?> GetByIdWithRulesTrackedAsync(
        int id, CancellationToken cancellationToken = default)
        => await Context.Set<Coupon>()
            .Include(c => c.ApplicableCategories)
            .Include(c => c.ApplicableProducts)
            .Include(c => c.ApplicableVariants)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task ReplaceApplicabilityRulesAsync(
        int couponId,
        int[] categoryIds, int[] productIds, int[] variantIds,
        CancellationToken cancellationToken = default)
    {
        // Delete existing
        await Context.CouponApplicableCategories
            .Where(x => x.CouponId == couponId)
            .ExecuteDeleteAsync(cancellationToken);

        await Context.CouponApplicableProducts
            .Where(x => x.CouponId == couponId)
            .ExecuteDeleteAsync(cancellationToken);

        await Context.CouponApplicableVariants
            .Where(x => x.CouponId == couponId)
            .ExecuteDeleteAsync(cancellationToken);

        // Insert new
        foreach (var catId in categoryIds ?? [])
            Context.CouponApplicableCategories.Add(
                new CouponApplicableCategory { CouponId = couponId, CategoryId = catId });

        foreach (var prodId in productIds ?? [])
            Context.CouponApplicableProducts.Add(
                new CouponApplicableProduct { CouponId = couponId, ProductId = prodId });

        foreach (var varId in variantIds ?? [])
            Context.CouponApplicableVariants.Add(
                new CouponApplicableVariant { CouponId = couponId, VariantId = varId });
    }

    public async Task<(List<Coupon> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, bool? isActive, string sortBy,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Coupon>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.Code.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy.ToLower() switch
        {
            "code-asc"       => query.OrderBy(c => c.Code),
            "usedcount-desc" => query.OrderByDescending(c => c.UsedCount),
            "expiresat-asc"  => query.OrderBy(c => c.ExpiresAt),
            _                => query.OrderByDescending(c => c.CreatedAt)
        };

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Coupon?> GetByIdWithApplicabilityAsync(int id, CancellationToken cancellationToken = default)
    {
        var raw = await Context.Set<Coupon>()
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.Code, c.DiscountType, c.DiscountValue,
                c.MinOrderAmount, c.MaxDiscountAmount,
                c.UsageLimit, c.PerUserLimit, c.UsedCount,
                c.IsActive, c.StartsAt, c.ExpiresAt,
                c.CreatedAt, c.UpdatedAt,
                Categories = c.ApplicableCategories
                    .Select(ac => new { ac.CategoryId, ac.Category.Name }).ToList(),
                Products = c.ApplicableProducts
                    .Select(ap => new { ap.ProductId, ap.Product.Title }).ToList(),
                Variants = c.ApplicableVariants
                    .Select(av => new { av.VariantId, av.Variant.Sku }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (raw is null) return null;

        return new Coupon
        {
            Id = raw.Id, Code = raw.Code, DiscountType = raw.DiscountType,
            DiscountValue = raw.DiscountValue, MinOrderAmount = raw.MinOrderAmount,
            MaxDiscountAmount = raw.MaxDiscountAmount, UsageLimit = raw.UsageLimit,
            PerUserLimit = raw.PerUserLimit, UsedCount = raw.UsedCount,
            IsActive = raw.IsActive, StartsAt = raw.StartsAt, ExpiresAt = raw.ExpiresAt,
            CreatedAt = raw.CreatedAt, UpdatedAt = raw.UpdatedAt,
            ApplicableCategories = raw.Categories
                .Select(c => new CouponApplicableCategory
                {
                    CategoryId = c.CategoryId,
                    Category   = new Category { Name = c.Name }
                }).ToList(),
            ApplicableProducts = raw.Products
                .Select(p => new CouponApplicableProduct
                {
                    ProductId = p.ProductId,
                    Product   = new Product { Title = p.Title }
                }).ToList(),
            ApplicableVariants = raw.Variants
                .Select(v => new CouponApplicableVariant
                {
                    VariantId = v.VariantId,
                    Variant   = new ProductVariant { Sku = v.Sku }
                }).ToList()
        };
    }

    public async Task<(int TotalUsed, int UniqueUsers, decimal TotalDiscountGiven)> GetUsageStatsAsync(
        int couponId, CancellationToken cancellationToken = default)
    {
        var usages = Context.CouponUsages.AsNoTracking().Where(u => u.CouponId == couponId);
        var totalUsed = await usages.CountAsync(cancellationToken);
        var uniqueUsers = await usages.Where(u => u.UserId != null)
            .Select(u => u.UserId).Distinct().CountAsync(cancellationToken);
        var totalDiscountGiven = await usages.SumAsync(u => u.DiscountApplied, cancellationToken);
        return (totalUsed, uniqueUsers, totalDiscountGiven);
    }

    public async Task<List<CouponUsage>> GetRecentUsageDataAsync(
        int couponId, int take, CancellationToken cancellationToken = default)
        => await Context.CouponUsages
            .AsNoTracking()
            .Include(u => u.Order)
            .Where(u => u.CouponId == couponId)
            .OrderByDescending(u => u.UsedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<(List<CouponUsage> Items, int TotalCount)> GetUsagesPagedAsync(
        int couponId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Context.CouponUsages.AsNoTracking().Where(u => u.CouponId == couponId);

        var totalCount = await query.CountAsync(cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .Include(u => u.Order)
            .OrderByDescending(u => u.UsedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
