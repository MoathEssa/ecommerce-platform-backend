using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Inventory.Queries.GetInventoryDetail;

public class GetInventoryDetailQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    IInventoryAdjustmentRepository adjustmentRepository)
    : IRequestHandler<GetInventoryDetailQuery, Result<InventoryDetailDto>>
{
    public async Task<Result<InventoryDetailDto>> Handle(
        GetInventoryDetailQuery request,
        CancellationToken cancellationToken)
    {
        var item = await inventoryItemRepository.GetWithNavigationByVariantIdAsync(
            request.VariantId, cancellationToken);

        if (item is null)
            return Result<InventoryDetailDto>.NotFound("InventoryItem", request.VariantId);

        var adjustments = await adjustmentRepository.GetRecentByVariantIdAsync(
            request.VariantId, 20, cancellationToken);

        var dto = new InventoryDetailDto(
            item.VariantId,
            item.Variant.Sku,
            item.Variant.Product.Title,
            item.OnHand,
            item.OnHand,
            adjustments.Select(a => new AdjustmentDto(a.Id, a.Delta, a.Reason, a.ActorId, a.CreatedAt)).ToList());

        return Result<InventoryDetailDto>.Success(dto);
    }
}
