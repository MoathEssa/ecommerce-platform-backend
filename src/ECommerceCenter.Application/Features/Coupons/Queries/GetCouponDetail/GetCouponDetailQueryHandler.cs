using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Queries.GetCouponDetail;

public class GetCouponDetailQueryHandler(ICouponRepository couponRepository)
    : IRequestHandler<GetCouponDetailQuery, Result<CouponDetailDto>>
{
    public async Task<Result<CouponDetailDto>> Handle(
        GetCouponDetailQuery request, CancellationToken cancellationToken)
    {
        var coupon = await couponRepository.GetByIdWithApplicabilityAsync(request.Id, cancellationToken);
        if (coupon is null)
            return Result<CouponDetailDto>.NotFound("Coupon", request.Id);

        var (totalUsed, uniqueUsers, totalDiscountGiven) =
            await couponRepository.GetUsageStatsAsync(request.Id, cancellationToken);

        var recentUsages = await couponRepository.GetRecentUsageDataAsync(request.Id, 10, cancellationToken);

        var dto = new CouponDetailDto(
            coupon.Id, coupon.Code, coupon.DiscountType.ToString(), coupon.DiscountValue,
            coupon.MinOrderAmount, coupon.MaxDiscountAmount,
            coupon.UsageLimit, coupon.PerUserLimit, coupon.UsedCount, coupon.IsActive,
            coupon.StartsAt, coupon.ExpiresAt,
            coupon.ApplicableCategories.Select(ac =>
                new CouponCategoryRefDto(ac.CategoryId, ac.Category.Name)).ToList(),
            coupon.ApplicableProducts.Select(ap =>
                new CouponProductRefDto(ap.ProductId, ap.Product.Title)).ToList(),
            coupon.ApplicableVariants.Select(av =>
                new CouponVariantRefDto(av.VariantId, av.Variant.Sku)).ToList(),
            new CouponUsageStatsDto(
                totalUsed, uniqueUsers, totalDiscountGiven,
                recentUsages.Select(u => new CouponUsageItemDto(
                    u.OrderId, u.Order.OrderNumber, u.UserId, u.Order.Email,
                    u.DiscountApplied, u.UsedAt)).ToList()),
            coupon.CreatedAt, coupon.UpdatedAt);

        return Result<CouponDetailDto>.Success(dto);
    }
}
