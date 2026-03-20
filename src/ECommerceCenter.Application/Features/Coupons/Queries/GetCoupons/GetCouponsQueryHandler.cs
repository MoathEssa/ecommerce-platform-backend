using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Queries.GetCoupons;

public class GetCouponsQueryHandler(ICouponRepository couponRepository)
    : IRequestHandler<GetCouponsQuery, Result<PaginatedList<CouponListItemDto>>>
{
    public async Task<Result<PaginatedList<CouponListItemDto>>> Handle(
        GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await couponRepository.GetPagedAsync(
            page, pageSize, request.Search, request.IsActive, request.SortBy, cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var dtos = items.Select(c => new CouponListItemDto(
            c.Id, c.Code, c.DiscountType.ToString(), c.DiscountValue,
            c.MinOrderAmount, c.MaxDiscountAmount,
            c.UsageLimit, c.PerUserLimit, c.UsedCount, c.IsActive,
            c.StartsAt, c.ExpiresAt, c.CreatedAt)).ToList();

        return Result<PaginatedList<CouponListItemDto>>.Success(
            new PaginatedList<CouponListItemDto>(dtos, page, pageSize, totalCount, totalPages));
    }
}
