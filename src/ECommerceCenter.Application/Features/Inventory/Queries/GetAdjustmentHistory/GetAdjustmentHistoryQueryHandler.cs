using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Inventory.Queries.GetAdjustmentHistory;

public class GetAdjustmentHistoryQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    IInventoryAdjustmentRepository adjustmentRepository)
    : IRequestHandler<GetAdjustmentHistoryQuery, Result<PaginatedList<AdjustmentDto>>>
{
    public async Task<Result<PaginatedList<AdjustmentDto>>> Handle(
        GetAdjustmentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var item = await inventoryItemRepository.GetByVariantIdAsync(request.VariantId, cancellationToken);
        if (item is null)
            return Result<PaginatedList<AdjustmentDto>>.NotFound("InventoryItem", request.VariantId);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var totalCount = await adjustmentRepository.CountByVariantIdAsync(request.VariantId, cancellationToken);
       
        var adjustments = await adjustmentRepository.GetByVariantIdPagedAsync(
            request.VariantId, page, pageSize, cancellationToken);

        var dtos = adjustments.Select(a =>
            new AdjustmentDto(a.Id, a.Delta, a.Reason, a.ActorId, a.CreatedAt)).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Result<PaginatedList<AdjustmentDto>>.Success(
            new PaginatedList<AdjustmentDto>(dtos, page, pageSize, totalCount, totalPages));
    }
}
