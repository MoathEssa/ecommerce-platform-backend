using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Commands.CreateCoupon;

public record CreateCouponCommand(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int PerUserLimit,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    int[] ApplicableCategories,
    int[] ApplicableProducts,
    int[] ApplicableVariants,
    int ActorId) : IRequest<Result<CouponDetailDto>>;
