using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.UpdateVariant;

public class UpdateVariantCommandHandler(
    IProductVariantRepository variantRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<UpdateVariantCommand, Result<AdminProductVariantDto>>
{
    public async Task<Result<AdminProductVariantDto>> Handle(
        UpdateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await variantRepository
            .GetByIdAndProductAsync(request.VariantId, request.ProductId, cancellationToken);

        if (variant is null)
            return Result<AdminProductVariantDto>.NotFound("ProductVariant", request.VariantId);

        var beforeJson = JsonSerializer.Serialize(new
        {
            variant.Sku, variant.BasePrice, variant.CurrencyCode, variant.IsActive
        });

        if (!string.Equals(variant.Sku, request.Sku, StringComparison.OrdinalIgnoreCase))
        {
            var skuTaken = await variantRepository.SkuExistsAsync(request.Sku, variant.Id, cancellationToken);
            if (skuTaken)
                return Result<AdminProductVariantDto>.Duplicate("Sku", request.Sku);
        }

        variant.Sku          = request.Sku.Trim();
        variant.OptionsJson  = JsonSerializer.Serialize(request.Options);
        variant.BasePrice    = request.BasePrice;
        variant.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        variant.IsActive     = request.IsActive;
        variant.UpdatedAt    = DateTime.UtcNow;

        variantRepository.Update(variant);
        await auditLogs.AddAsync(new AuditLog
        {
            ActorId    = currentUserService.UserId,
            ActorType  = ActorType.Admin,
            Action     = "Variant.Update",
            EntityType = "ProductVariant",
            EntityId   = variant.Id,
            BeforeJson = beforeJson,
            AfterJson  = JsonSerializer.Serialize(new
            {
                variant.Sku, variant.BasePrice, variant.CurrencyCode, variant.IsActive
            }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var inv = variant.InventoryItem;
        var onHand = inv?.OnHand ?? 0;

        return Result<AdminProductVariantDto>.Success(new AdminProductVariantDto(
            variant.Id, variant.Sku, request.Options,
            variant.BasePrice, variant.SupplierPrice, variant.CurrencyCode, variant.IsActive,
            new AdminInventoryDto(onHand, onHand)));
    }
}
