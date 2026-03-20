using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Catalog;

public class ProductRepository(AppDbContext context)
    : GenericRepository<Product>(context), IProductRepository
{
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await Context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

    public async Task<bool> SlugExistsAsync(
        string slug, int? excludeId, CancellationToken cancellationToken = default)
        => await Context.Products
            .AnyAsync(p => p.Slug == slug && (excludeId == null || p.Id != excludeId.Value), cancellationToken);

    public async Task<long> NextSlugSuffixAsync(CancellationToken cancellationToken = default)
        => await Context.Database
            .SqlQuery<long>($"SELECT NEXT VALUE FOR slug_suffix_seq AS Value")
            .FirstAsync(cancellationToken);

    public async Task<Product?> FindCjProductWithVariantsAsync(
        string cjProductId, CancellationToken ct = default)
        => await Context.Set<Product>()
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(
                p => p.ExternalProductId == cjProductId &&
                     p.Supplier == SupplierType.CjDropshipping,
                ct);

    // ── Read projections ───────────────────────────────────────────────────

    public async Task<(List<ProductListingRow> Items, int TotalCount)> GetProductListingPagedAsync(
        ProductListFilter filter, IReadOnlyCollection<int>? categoryIds, CancellationToken ct = default)
    {
        var query = Context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active)
            .Where(p => p.Variants.Any(v => v.IsActive));

        if (categoryIds is not null)
            query = query.Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p =>
                p.Title.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)) ||
                (p.Brand != null && p.Brand.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand != null && p.Brand == filter.Brand);

        if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
            query = query.Where(p => p.Variants.Any(v =>
                v.IsActive &&
                (!filter.MinPrice.HasValue || v.BasePrice >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || v.BasePrice <= filter.MaxPrice.Value)));

        if (filter.InStock == true)
            query = query.Where(p => p.Variants.Any(v =>
                v.IsActive &&
                v.InventoryItem != null &&
                (v.InventoryItem.OnHand) > 0));

        var totalCount = await query.CountAsync(ct);

        IOrderedQueryable<Product> sorted = filter.SortBy switch
        {
            "price-asc"  => query.OrderBy(p => p.Variants.Where(v => v.IsActive).Min(v => v.BasePrice)),
            "price-desc" => query.OrderByDescending(p => p.Variants.Where(v => v.IsActive).Min(v => v.BasePrice)),
            "name-asc"   => query.OrderBy(p => p.Title),
            _            => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await sorted
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductListingRow(
                p.Id, p.Title, p.Slug, p.Brand,
                p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                p.Variants.Where(v => v.IsActive).Min(v => v.BasePrice),
                p.Variants.Where(v => v.IsActive).Max(v => v.BasePrice),
                p.Variants.Where(v => v.IsActive).Select(v => v.CurrencyCode).FirstOrDefault() ?? "SAR",
                p.Variants.Any(v =>
                    v.IsActive &&
                    v.InventoryItem != null &&
                    (v.InventoryItem.OnHand) > 0),
                p.CategoryId,
                p.Category != null ? p.Category.Name : null,
                p.Category != null ? p.Category.Slug : null))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<ProductDetailRow?> GetProductDetailBySlugAsync(string slug, CancellationToken ct = default)
    {
        var raw = await Context.Set<Product>()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p =>
                p.Slug == slug &&
                p.Status == ProductStatus.Active &&
                p.Variants.Any(v => v.IsActive))
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Description, p.Brand,
                Status = (int)p.Status,
                p.ExternalProductId,
                Supplier = (int?)p.Supplier,
                CategoryId   = (int?)p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                CategorySlug = p.Category != null ? p.Category.Slug : null,
                Variants = p.Variants
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.Id)
                    .Select(v => new
                    {
                        v.Id, v.Sku, v.OptionsJson, v.BasePrice, v.SupplierPrice, v.CurrencyCode, v.IsActive,
                        OnHand   = v.InventoryItem != null ? v.InventoryItem.OnHand   : 0
                    })
                    .ToList(),
                Images = p.Images
                    .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
                    .Select(i => new { i.Id, i.Url, i.VariantId, i.SortOrder })
                    .ToList(),
                p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (raw is null) return null;

        var cat = raw.CategoryId.HasValue && raw.CategoryName is not null
            ? new CatalogCategoryRefRow(raw.CategoryId.Value, raw.CategoryName, raw.CategorySlug!)
            : (CatalogCategoryRefRow?)null;

        return new ProductDetailRow(
            raw.Id, raw.Title, raw.Slug, raw.Description, raw.Brand, raw.Status,
            raw.ExternalProductId, raw.Supplier,
            cat,
            raw.Variants.Select(v => new CatalogVariantRow(v.Id, v.Sku, v.OptionsJson, v.BasePrice, v.SupplierPrice, v.CurrencyCode, v.IsActive, v.OnHand)).ToList(),
            raw.Images.Select(i => new CatalogImageRow(i.Id, i.Url, i.VariantId, i.SortOrder)).ToList(),
            raw.CreatedAt);
    }

    public async Task<List<CatalogSearchRow>> SearchByTitleAsync(
        string term, int limit, CancellationToken ct = default)
        => await Context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active && p.Title.StartsWith(term))
            .OrderBy(p => p.Title)
            .Take(limit)
            .Select(p => new CatalogSearchRow(p.Title, p.Slug))
            .ToListAsync(ct);

    public async Task<(List<AdminProductListingRow> Items, int TotalCount)> GetAdminProductListingPagedAsync(
        AdminProductListFilter filter, CancellationToken ct = default)
    {
        var query = Context.Set<Product>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<ProductStatus>(filter.Status, ignoreCase: true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p =>
                p.Title.Contains(term) ||
                p.Slug.Contains(term) ||
                (p.Brand != null && p.Brand.Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        IOrderedQueryable<Product> sorted = filter.SortBy switch
        {
            "title-asc"  => query.OrderBy(p => p.Title),
            "title-desc" => query.OrderByDescending(p => p.Title),
            _            => query.OrderByDescending(p => p.CreatedAt)
        };

        var rawItems = await sorted
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Brand, p.Status,
                ActiveVariantCount = p.Variants.Count(v => v.IsActive),
                CoverImageUrl = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                p.CreatedAt
            })
            .ToListAsync(ct);

        var items = rawItems
            .Select(r => new AdminProductListingRow(
                r.Id, r.Title, r.Slug, r.Brand, (int)r.Status,
                r.ActiveVariantCount, r.CoverImageUrl, r.CreatedAt))
            .ToList();

        return (items, totalCount);
    }

    public async Task<AdminProductDetailRow?> GetAdminProductDetailByIdAsync(int id, CancellationToken ct = default)
    {
        var raw = await Context.Set<Product>()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Description, p.Brand,
                p.Status,
                p.ExternalProductId,
                Supplier = (int?)p.Supplier,
                CategoryId   = (int?)p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                CategorySlug = p.Category != null ? p.Category.Slug : null,
                Variants = p.Variants
                    .OrderBy(v => v.Id)
                    .Select(v => new
                    {
                        v.Id, v.Sku, v.OptionsJson, v.BasePrice, v.SupplierPrice, v.CurrencyCode, v.IsActive,
                        OnHand   = v.InventoryItem != null ? v.InventoryItem.OnHand   : 0
                    })
                    .ToList(),
                Images = p.Images
                    .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
                    .Select(i => new { i.Id, i.Url, i.VariantId, i.SortOrder })
                    .ToList(),
                p.CreatedAt, p.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (raw is null) return null;

        var adminCat = raw.CategoryId.HasValue && raw.CategoryName is not null
            ? new CatalogCategoryRefRow(raw.CategoryId.Value, raw.CategoryName, raw.CategorySlug!)
            : (CatalogCategoryRefRow?)null;

        return new AdminProductDetailRow(
            raw.Id, raw.Title, raw.Slug, raw.Description, raw.Brand, (int)raw.Status,
            raw.ExternalProductId, raw.Supplier,
            adminCat,
            raw.Variants.Select(v => new CatalogVariantRow(v.Id, v.Sku, v.OptionsJson, v.BasePrice, v.SupplierPrice, v.CurrencyCode, v.IsActive, v.OnHand)).ToList(),
            raw.Images.Select(i => new CatalogImageRow(i.Id, i.Url, i.VariantId, i.SortOrder)).ToList(),
            raw.CreatedAt, raw.UpdatedAt);
    }
}
