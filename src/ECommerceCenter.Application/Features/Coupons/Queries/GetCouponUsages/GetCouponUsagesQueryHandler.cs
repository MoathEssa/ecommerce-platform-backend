using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Queries.GetCouponUsages;

public class GetCouponUsagesQueryHandler(ICouponRepository couponRepository)
    : IRequestHandler<GetCouponUsagesQuery, Result<PaginatedList<CouponUsageItemDto>>>
{
    public async Task<Result<PaginatedList<CouponUsageItemDto>>> Handle(
        GetCouponUsagesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (rows, totalCount) = await couponRepository.GetUsagesPagedAsync(
            request.CouponId, page, pageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var dtos = rows.Select(u => new CouponUsageItemDto(
            u.OrderId, u.Order.OrderNumber, u.UserId, u.Order.Email,
            u.DiscountApplied, u.UsedAt)).ToList();

        return Result<PaginatedList<CouponUsageItemDto>>.Success(
            new PaginatedList<CouponUsageItemDto>(dtos, page, pageSize, totalCount, totalPages));
    }
}
