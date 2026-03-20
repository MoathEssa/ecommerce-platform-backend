using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Inventory.Queries.GetAdjustmentHistory;

public record GetAdjustmentHistoryQuery(
    int VariantId,
    int Page,
    int PageSize) : IRequest<Result<PaginatedList<AdjustmentDto>>>;
