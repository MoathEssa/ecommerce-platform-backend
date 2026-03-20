using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Domain.Entities.Inventory;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Inventory;

public class InventoryAdjustmentRepository(AppDbContext context)
    : GenericRepository<InventoryAdjustment>(context), IInventoryAdjustmentRepository
{
    public async Task<IEnumerable<InventoryAdjustment>> GetByVariantIdPagedAsync(
        int variantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => await Context.Set<InventoryAdjustment>()
            .Where(a => a.VariantId == variantId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByVariantIdAsync(
        int variantId,
        CancellationToken cancellationToken = default)
        => await Context.Set<InventoryAdjustment>()
            .CountAsync(a => a.VariantId == variantId, cancellationToken);

    public async Task<List<InventoryAdjustment>> GetRecentByVariantIdAsync(
        int variantId, int take, CancellationToken cancellationToken = default)
        => await Context.Set<InventoryAdjustment>()
            .AsNoTracking()
            .Where(a => a.VariantId == variantId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
