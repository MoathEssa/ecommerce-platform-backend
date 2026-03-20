using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Queries.GetCouponUsages;

public record GetCouponUsagesQuery(
    int CouponId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<CouponUsageItemDto>>>;
