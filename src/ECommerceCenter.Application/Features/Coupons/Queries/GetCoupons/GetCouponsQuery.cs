using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Queries.GetCoupons;

public record GetCouponsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null,
    string SortBy = "newest") : IRequest<Result<PaginatedList<CouponListItemDto>>>;
