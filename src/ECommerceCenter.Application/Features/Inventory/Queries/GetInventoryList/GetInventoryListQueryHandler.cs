using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Inventory.Queries.GetInventoryList;

public class GetInventoryListQueryHandler(IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<GetInventoryListQuery, Result<PaginatedList<InventoryListItemDto>>>
{
    public async Task<Result<PaginatedList<InventoryListItemDto>>> Handle(
        GetInventoryListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await inventoryItemRepository.GetPagedAsync(
            page, pageSize, request.Search, request.StockStatus, request.SortBy,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var dtos = items.Select(i => new InventoryListItemDto(
            i.VariantId,
            i.Variant.Sku!,
            i.Variant.Product.Title,
            i.Variant.OptionsJson,
            i.OnHand,
            i.OnHand,
            StockThresholds.Map(i.OnHand),
            i.UpdatedAt)).ToList();

        return Result<PaginatedList<InventoryListItemDto>>.Success(
            new PaginatedList<InventoryListItemDto>(dtos, page, pageSize, totalCount, totalPages));
    }
}
