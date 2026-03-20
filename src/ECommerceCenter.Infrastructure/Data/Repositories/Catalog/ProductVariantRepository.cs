using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Catalog;

public class ProductVariantRepository(AppDbContext context)
    : GenericRepository<ProductVariant>(context), IProductVariantRepository
{
    public async Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        => await Context.Set<ProductVariant>()
            .FirstOrDefaultAsync(v => v.Sku == sku, cancellationToken);

    // -- Admin -----------------------------------------------------------

    public async Task<ProductVariant?> GetByIdAndProductAsync(
        int variantId, int productId, CancellationToken cancellationToken = default)
        => await Context.Set<ProductVariant>()
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId, cancellationToken);

    public async Task<bool> SkuExistsAsync(
        string sku, int? excludeId, CancellationToken cancellationToken = default)
        => await Context.Set<ProductVariant>()
            .AnyAsync(v => v.Sku == sku && (excludeId == null || v.Id != excludeId.Value), cancellationToken);

    public async Task<bool> HasActiveVariantsAsync(
        int productId, CancellationToken cancellationToken = default)
        => await Context.Set<ProductVariant>()
            .AnyAsync(v => v.ProductId == productId && v.IsActive, cancellationToken);

    public async Task<long> NextSkuAsync(CancellationToken cancellationToken = default)
    {
        var connection = Context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR variant_sku_seq";
        command.Transaction = Context.Database.CurrentTransaction?.GetDbTransaction();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result!);
    }

    public async Task<ProductVariant?> GetWithProductAsync(int variantId, CancellationToken cancellationToken = default)
        => await Context.Set<ProductVariant>()
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);

    public async Task<Dictionary<int, ProductVariant>> GetWithProductsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default)
        => await Context.Set<ProductVariant>()
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

    public async Task<Dictionary<int, int>> GetProductIdsByVariantIdsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default)
        => await Context.Set<ProductVariant>()
            .AsNoTracking()
            .Where(v => variantIds.Contains(v.Id))
            .Select(v => new { v.Id, v.ProductId })
            .ToDictionaryAsync(v => v.Id, v => v.ProductId, cancellationToken);

    public async Task<VariantDetailRow?> GetVariantDetailAsync(
        string productSlug, int variantId, CancellationToken ct = default)
    {
        var raw = await Context.Set<ProductVariant>()
            .AsNoTracking()
            .Where(v => v.Product.Slug == productSlug
                        && v.Product.Status == ProductStatus.Active
                        && v.Id == variantId
                        && v.IsActive)
            .Select(v => new
            {
                v.Id, v.Sku, v.OptionsJson, v.BasePrice, v.CurrencyCode,
                OnHand   = v.InventoryItem != null ? v.InventoryItem.OnHand   : 0,
                Images   = v.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new { i.Id, i.Url, VariantId = v.Id, i.SortOrder })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (raw is null) return null;

        return new VariantDetailRow(
            raw.Id, raw.Sku, raw.OptionsJson, raw.BasePrice, raw.CurrencyCode,
            raw.OnHand,
            raw.Images.Select(i => new CatalogImageRow(i.Id, i.Url, i.VariantId, i.SortOrder)).ToList());
    }
}
