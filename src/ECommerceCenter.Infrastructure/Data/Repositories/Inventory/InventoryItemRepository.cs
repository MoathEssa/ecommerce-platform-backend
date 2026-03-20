using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Domain.Entities.Inventory;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Inventory;

public class InventoryItemRepository(AppDbContext context)
    : GenericRepository<InventoryItem>(context), IInventoryItemRepository
{
    public async Task<InventoryItem?> GetByVariantIdAsync(int variantId, CancellationToken cancellationToken = default)
        => await Context.Set<InventoryItem>()
            .FirstOrDefaultAsync(i => i.VariantId == variantId, cancellationToken);

    public async Task<Dictionary<int, InventoryItem>> GetByVariantIdsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default)
        => await Context.Set<InventoryItem>()
            .Where(i => variantIds.Contains(i.VariantId))
            .ToDictionaryAsync(i => i.VariantId, cancellationToken);

    public async Task<(List<InventoryItem> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? stockStatus, string sortBy,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<InventoryItem>()
            .AsNoTracking()
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.Variant.Sku.ToLower().Contains(term) ||
                i.Variant.Product.Title.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(stockStatus))
        {
            query = stockStatus.ToLower() switch
            {
                "instock" => query.Where(i => i.OnHand >= StockThresholds.InStock),
                "lowstock" => query.Where(i =>
                    i.OnHand >= StockThresholds.LowStock &&
                    i.OnHand < StockThresholds.InStock),
                "outofstock" => query.Where(i => i.OnHand <= 0),
                _ => query
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy.ToLower() switch
        {
            "onhand-asc" => query.OrderBy(i => i.OnHand),
            "onhand-desc" => query.OrderByDescending(i => i.OnHand),
            "available-asc" => query.OrderBy(i => i.OnHand),
            _ => query.OrderBy(i => i.Variant.Sku)
        };

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<InventoryItem?> GetWithNavigationByVariantIdAsync(
        int variantId, CancellationToken cancellationToken = default)
        => await Context.Set<InventoryItem>()
            .AsNoTracking()
            .Include(i => i.Variant)
                .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(i => i.VariantId == variantId, cancellationToken);
}
