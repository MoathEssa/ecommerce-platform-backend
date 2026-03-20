using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommandHandler(
    ICouponRepository couponRepository,
    IAuditLogRepository auditLogRepository,
    IEfUnitOfWork unitOfWork) : IRequestHandler<UpdateCouponCommand, Result<CouponDetailDto>>
{
    public async Task<Result<CouponDetailDto>> Handle(
        UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await couponRepository.GetByIdWithRulesTrackedAsync(request.Id, cancellationToken);
        if (coupon is null)
            return Result<CouponDetailDto>.NotFound("Coupon", request.Id);

        // Check code uniqueness (case-insensitive, excluding self)
        var duplicate = await couponRepository.ExistsAsync(
            c => c.Code.ToUpper() == request.Code.ToUpper().Trim() && c.Id != request.Id,
            cancellationToken);

        if (duplicate)
            return Result<CouponDetailDto>.BusinessRuleViolation(
                BusinessRuleCode.CouponCodeExists,
                $"Coupon code '{request.Code}' already exists.");

        if (!Enum.TryParse<DiscountType>(request.DiscountType, true, out var discountType))
            return Result<CouponDetailDto>.ValidationError("Invalid discount type.");

        if (discountType == DiscountType.Percentage && request.DiscountValue > 100)
            return Result<CouponDetailDto>.BusinessRuleViolation(
                BusinessRuleCode.CouponInvalidPercentage,
                "Percentage discount value must be between 0 and 100.");

        if (request.StartsAt.HasValue && request.ExpiresAt.HasValue
            && request.ExpiresAt <= request.StartsAt)
            return Result<CouponDetailDto>.BusinessRuleViolation(
                BusinessRuleCode.InvalidCouponDates,
                "ExpiresAt must be after StartsAt.");

        var beforeJson = JsonSerializer.Serialize(new
        {
            coupon.Code, DiscountType = coupon.DiscountType.ToString(),
            coupon.DiscountValue, coupon.IsActive
        });

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            coupon.Code = request.Code.Trim();
            coupon.DiscountType = discountType;
            coupon.DiscountValue = request.DiscountValue;
            coupon.MinOrderAmount = request.MinOrderAmount;
            coupon.MaxDiscountAmount = request.MaxDiscountAmount;
            coupon.UsageLimit = request.UsageLimit;
            coupon.PerUserLimit = request.PerUserLimit;
            coupon.IsActive = request.IsActive;
            coupon.StartsAt = request.StartsAt;
            coupon.ExpiresAt = request.ExpiresAt;
            coupon.UpdatedAt = DateTime.UtcNow;

            couponRepository.Update(coupon);

            // Replace applicability rules
            await couponRepository.ReplaceApplicabilityRulesAsync(
                coupon.Id,
                request.ApplicableCategories ?? [],
                request.ApplicableProducts ?? [],
                request.ApplicableVariants ?? [],
                ct);

            await auditLogRepository.AddAsync(new AuditLog
            {
                ActorId = request.ActorId,
                ActorType = ActorType.Admin,
                Action = "Coupon.Update",
                EntityType = "Coupon",
                EntityId = coupon.Id,
                BeforeJson = beforeJson,
                AfterJson = JsonSerializer.Serialize(new
                {
                    coupon.Code, DiscountType = coupon.DiscountType.ToString(),
                    coupon.DiscountValue, coupon.IsActive
                })
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        var dto = new CouponDetailDto(
            coupon.Id, coupon.Code, coupon.DiscountType.ToString(), coupon.DiscountValue,
            coupon.MinOrderAmount, coupon.MaxDiscountAmount,
            coupon.UsageLimit, coupon.PerUserLimit, coupon.UsedCount, coupon.IsActive,
            coupon.StartsAt, coupon.ExpiresAt,
            (request.ApplicableCategories ?? []).Select(id => new CouponCategoryRefDto(id, "")).ToList(),
            (request.ApplicableProducts ?? []).Select(id => new CouponProductRefDto(id, "")).ToList(),
            (request.ApplicableVariants ?? []).Select(id => new CouponVariantRefDto(id, "")).ToList(),
            null, coupon.CreatedAt, coupon.UpdatedAt);

        return Result<CouponDetailDto>.Success(dto);
    }
}
