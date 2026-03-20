using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Coupons;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandler(
    ICouponRepository couponRepository,
    IAuditLogRepository auditLogRepository,
    IEfUnitOfWork unitOfWork) : IRequestHandler<CreateCouponCommand, Result<CouponDetailDto>>
{
    public async Task<Result<CouponDetailDto>> Handle(
        CreateCouponCommand request, CancellationToken cancellationToken)
    {
        // Check code uniqueness (case-insensitive)
        var exists = await couponRepository.ExistsAsync(
            c => c.Code.ToUpper() == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
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

        var coupon = new Coupon
        {
            Code = request.Code.Trim(),
            DiscountType = discountType,
            DiscountValue = request.DiscountValue,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            PerUserLimit = request.PerUserLimit,
            IsActive = request.IsActive,
            StartsAt = request.StartsAt,
            ExpiresAt = request.ExpiresAt
        };

        // Applicability rules
        foreach (var catId in request.ApplicableCategories ?? [])
            coupon.ApplicableCategories.Add(new CouponApplicableCategory { CategoryId = catId });

        foreach (var prodId in request.ApplicableProducts ?? [])
            coupon.ApplicableProducts.Add(new CouponApplicableProduct { ProductId = prodId });

        foreach (var varId in request.ApplicableVariants ?? [])
            coupon.ApplicableVariants.Add(new CouponApplicableVariant { VariantId = varId });

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await couponRepository.AddAsync(coupon, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await auditLogRepository.AddAsync(new AuditLog
            {
                ActorId = request.ActorId,
                ActorType = ActorType.Admin,
                Action = "Coupon.Create",
                EntityType = "Coupon",
                EntityId = coupon.Id,
                AfterJson = JsonSerializer.Serialize(new { coupon.Code, coupon.DiscountType, coupon.DiscountValue })
            }, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        // Build response DTO
        var dto = new CouponDetailDto(
            coupon.Id, coupon.Code, coupon.DiscountType.ToString(), coupon.DiscountValue,
            coupon.MinOrderAmount, coupon.MaxDiscountAmount,
            coupon.UsageLimit, coupon.PerUserLimit, coupon.UsedCount, coupon.IsActive,
            coupon.StartsAt, coupon.ExpiresAt,
            coupon.ApplicableCategories.Select(ac => new CouponCategoryRefDto(ac.CategoryId, "")).ToList(),
            coupon.ApplicableProducts.Select(ap => new CouponProductRefDto(ap.ProductId, "")).ToList(),
            coupon.ApplicableVariants.Select(av => new CouponVariantRefDto(av.VariantId, "")).ToList(),
            null, coupon.CreatedAt, coupon.UpdatedAt);

        return Result<CouponDetailDto>.Success(dto);
    }
}
