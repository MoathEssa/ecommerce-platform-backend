using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Entities.Inventory;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.AddVariant;

public class AddVariantCommandHandler(
    IProductRepository productRepository,
    IProductVariantRepository variantRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs,
    IGenericRepository<InventoryItem> inventoryItems,
    IGenericRepository<InventoryAdjustment> adjustments)
    : IRequestHandler<AddVariantCommand, Result<AdminVariantCreatedDto>>
{
    public async Task<Result<AdminVariantCreatedDto>> Handle(
        AddVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<AdminVariantCreatedDto>.NotFound("Product", request.ProductId);

        ProductVariant variant = null!;
        InventoryItem  inventory = null!;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var skuSeq = await variantRepository.NextSkuAsync(ct);
            var sku    = $"SKU-{skuSeq:D6}";

            variant = new ProductVariant
            {
                ProductId    = request.ProductId,
                Sku          = sku,
                OptionsJson  = JsonSerializer.Serialize(request.Options),
                BasePrice    = request.BasePrice,
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                IsActive     = request.IsActive,
                CreatedAt    = DateTime.UtcNow
            };

            await variantRepository.AddAsync(variant, ct);
            await unitOfWork.SaveChangesAsync(ct); // variant.Id populated

            inventory = new InventoryItem
            {
                VariantId = variant.Id,
                OnHand    = request.InitialStock,
                UpdatedAt = DateTime.UtcNow
            };

            await inventoryItems.AddAsync(inventory, ct);

            if (request.InitialStock > 0)
            {
                await adjustments.AddAsync(new InventoryAdjustment
                {
                    VariantId = variant.Id,
                    Delta     = request.InitialStock,
                    Reason    = "Initial stock",
                    ActorId   = currentUserService.UserId,
                    CreatedAt = DateTime.UtcNow
                }, ct);
            }

            await auditLogs.AddAsync(new AuditLog
            {
                ActorId    = currentUserService.UserId,
                ActorType  = ActorType.Admin,
                Action     = "Variant.Create",
                EntityType = "ProductVariant",
                EntityId   = variant.Id,
                AfterJson  = JsonSerializer.Serialize(new
                {
                    variant.Id, variant.ProductId, variant.Sku,
                    variant.BasePrice, variant.CurrencyCode, variant.IsActive,
                    InitialStock = request.InitialStock
                }),
                CreatedAt = DateTime.UtcNow
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return Result<AdminVariantCreatedDto>.Success(new AdminVariantCreatedDto(
            variant.Id, variant.ProductId, variant.Sku!,
            request.Options,
            variant.BasePrice, variant.SupplierPrice, variant.CurrencyCode, variant.IsActive,
            new AdminInventoryDto(inventory.OnHand, inventory.OnHand)));
    }
}
