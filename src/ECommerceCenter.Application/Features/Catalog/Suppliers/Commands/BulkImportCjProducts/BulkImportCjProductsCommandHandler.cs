using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Entities.Inventory;
using ECommerceCenter.Domain.Enums;
using MediatR;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.BulkImportCjProducts;

public class BulkImportCjProductsCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ICjProductService cjProductService,
    IInventoryItemRepository inventoryItemRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<BulkImportCjProductsCommand, Result<BulkImportResult>>
{
    public async Task<Result<BulkImportResult>> Handle(
        BulkImportCjProductsCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            return Result<BulkImportResult>.BusinessRuleViolation(
                SupplierProductAlreadyImported,
                "No variants were selected for import.");

        // 1. Category mapping check — done once for all items ─────────────────
        Category? storeCategory = null;
        if (!string.IsNullOrWhiteSpace(request.CjCategoryId))
        {
            var matching = await categoryRepository.FindAllAsync(
                c => c.ExternalCategoryId == request.CjCategoryId &&
                     c.Supplier == SupplierType.CjDropshipping,
                cancellationToken: cancellationToken);

            storeCategory = matching.FirstOrDefault();

            if (storeCategory is null)
            {
                var displayName = request.ThreeCategoryName
                    ?? request.TwoCategoryName
                    ?? request.OneCategoryName
                    ?? request.CjCategoryId;

                return Result<BulkImportResult>.BusinessRuleViolation(
                    SupplierCategoryNotMapped,
                    $"The category \"{displayName}\" has not been imported into your store yet. " +
                    "Please import the category first, then retry importing this product.");
            }
        }

        // 2. Find or create the parent product (ONE time), then add variants ───
        int imported = 0;
        int skipped  = 0;
        var errors   = new List<string>();
        var newVariants = new List<ProductVariant>();

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var product = await productRepository.FindCjProductWithVariantsAsync(
                request.CjProductId, ct);

            bool isNewProduct = product is null;
            if (isNewProduct)
            {
                var title    = request.ProductNameEn.Trim();
                var baseSlug = SlugHelper.Generate(title);
                var slug     = await ResolveUniqueSlugAsync(baseSlug, ct);

                product = new Product
                {
                    Title             = title,
                    Slug              = slug,
                    Supplier          = SupplierType.CjDropshipping,
                    ExternalProductId = request.CjProductId,
                    Status            = request.MakeActive ? ProductStatus.Active : ProductStatus.Draft,
                    CreatedAt         = DateTime.UtcNow,
                };

                await productRepository.AddAsync(product, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }

            // 3. Build all new variants in memory — no per-row saves ──────────
            var existingCjIds = product!.Variants
                .Where(v => v.ExternalSkuId != null)
                .Select(v => v.ExternalSkuId!)
                .ToHashSet(StringComparer.Ordinal);

            int imageOffset = product.Images.Count;

            foreach (var item in request.Items)
            {
                if (existingCjIds.Contains(item.CjVariantId))
                {
                    skipped++;
                    continue;
                }

                decimal.TryParse(item.CjPrice,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var supplierPrice);

                if (!decimal.TryParse(item.SellPrice,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var price))
                {
                    errors.Add($"{item.VariantNameEn}: Invalid sell price — skipped.");
                    continue;
                }

                var variant = new ProductVariant
                {
                    BasePrice     = price,
                    SupplierPrice = supplierPrice > 0 ? supplierPrice : null,
                    CurrencyCode  = "USD",
                    IsActive      = true,
                    ExternalSkuId = item.CjVariantId,
                    OptionsJson   = "{}",
                    CreatedAt     = DateTime.UtcNow,
                };

                if (!string.IsNullOrWhiteSpace(item.ImageUrl))
                {
                    variant.Images.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        Url       = item.ImageUrl,
                        SortOrder = imageOffset++,
                        CreatedAt = DateTime.UtcNow,
                    });
                }

                product.Variants.Add(variant);
                newVariants.Add(variant);
                imported++;
            }

            // 4. One batch save: all variants + their images + category ────────
            if (imported > 0 || storeCategory is not null)
            {
                if (storeCategory is not null)
                    product.CategoryId = storeCategory.Id;

                await unitOfWork.SaveChangesAsync(ct);
            }

            // 5. Create inventory records + fetch CJ stock ─────────────────────
            foreach (var v in newVariants)
            {
                int stock = 0;
                if (!string.IsNullOrWhiteSpace(v.ExternalSkuId))
                {
                    var cjStock = await cjProductService.GetVariantStockAsync(v.ExternalSkuId, ct);
                    stock = cjStock ?? 0;
                }

                await inventoryItemRepository.AddAsync(new InventoryItem
                {
                    VariantId = v.Id,
                    OnHand    = stock,
                    UpdatedAt = DateTime.UtcNow,
                }, ct);
            }

            if (newVariants.Count > 0)
                await unitOfWork.SaveChangesAsync(ct);

        }, cancellationToken);

        var message = $"Bulk import complete: {imported} imported, {skipped} already existed.";
        return Result<BulkImportResult>.Success(
            new BulkImportResult(imported, skipped, errors), message);
    }

    private async Task<string> ResolveUniqueSlugAsync(string baseSlug, CancellationToken ct)
    {
        if (!await productRepository.SlugExistsAsync(baseSlug, null, ct))
            return baseSlug;

        var suffix = await productRepository.NextSlugSuffixAsync(ct);
        return $"{baseSlug}-{suffix}";
    }
}
