using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Inventory.Queries.GetInventoryList;

public record GetInventoryListQuery(
    int Page,
    int PageSize,
    string? Search,
    string? StockStatus,
    string SortBy) : IRequest<Result<PaginatedList<InventoryListItemDto>>>;
