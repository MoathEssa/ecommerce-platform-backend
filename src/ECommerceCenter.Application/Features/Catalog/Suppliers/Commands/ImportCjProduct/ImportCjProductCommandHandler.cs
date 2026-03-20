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

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.ImportCjProduct;

public class ImportCjProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ICjProductService cjProductService,
    IInventoryItemRepository inventoryItemRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<ImportCjProductCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ImportCjProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Prevent duplicate imports ────────────────────────────────────────
        var alreadyExists = await productRepository.ExistsAsync(
            p => p.ExternalProductId == request.CjProductId &&
                 p.Supplier == SupplierType.CjDropshipping,
            cancellationToken);

        if (alreadyExists)
            return Result<int>.Conflict(
                $"CJ product '{request.NameEn}' has already been imported into your store.");

        // 2. Category mapping check ────────────────────────────────────────────
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

                return Result<int>.BusinessRuleViolation(SupplierCategoryNotMapped,
                        $"The category \"{displayName}\" has not been imported into your store yet. " +
                        "Please import the category first, then retry importing this product.");
            }
        }

        // 3. Build the product ────────────────────────────────────────────────
        var title    = request.NameEn.Trim();
        var baseSlug = SlugHelper.Generate(title);
        var slug     = await ResolveUniqueSlugAsync(baseSlug, cancellationToken);

        var product = new Product
        {
            Title             = title,
            Slug              = slug,
            Supplier          = SupplierType.CjDropshipping,
            ExternalProductId = request.CjProductId,
            Status            = request.MakeActive ? ProductStatus.Active : ProductStatus.Draft,
            CreatedAt         = DateTime.UtcNow,
        };

        // 4. Default variant + image ──────────────────────────────────────────
        decimal.TryParse(request.CjPrice,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var supplierPrice);

        if (decimal.TryParse(request.SellPrice,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var price))
        {
            var variant = new ProductVariant
            {
                Sku           = string.IsNullOrWhiteSpace(request.Sku) ? null
                                    : request.Sku[..Math.Min(64, request.Sku.Length)],
                BasePrice     = price,
                SupplierPrice = supplierPrice > 0 ? supplierPrice : null,
                CurrencyCode  = "USD",
                IsActive      = true,
                ExternalSkuId = request.Sku,
                OptionsJson   = "{}",
                CreatedAt     = DateTime.UtcNow,
            };

            if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                var img = new ProductImage
                {
                    Url       = request.ImageUrl,
                    SortOrder = 0,
                    CreatedAt = DateTime.UtcNow,
                };
                product.Images.Add(img);
                variant.Images.Add(img);
            }

            product.Variants.Add(variant);
        }

        // 5. Persist in transaction ────────────────────────────────────────────
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (storeCategory is not null)
                product.CategoryId = storeCategory.Id;

            await productRepository.AddAsync(product, ct);
            await unitOfWork.SaveChangesAsync(ct);

            // 6. Create inventory records + fetch CJ stock ─────────────────────
            foreach (var v in product.Variants)
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

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return Result<int>.Success(product.Id);
    }

    private async Task<string> ResolveUniqueSlugAsync(string baseSlug, CancellationToken ct)
    {
        if (!await productRepository.SlugExistsAsync(baseSlug, null, ct))
            return baseSlug;

        var suffix = await productRepository.NextSlugSuffixAsync(ct);
        return $"{baseSlug}-{suffix}";
    }
}
