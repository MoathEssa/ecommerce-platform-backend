using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Domain.Enums;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;

namespace ECommerceCenter.Infrastructure.Services;

/// <summary>
/// Single source of truth for coupon eligibility and discount calculation.
/// Depends entirely on repository abstractions — never touches DbContext directly.
/// </summary>
public class CouponEvaluator(
    ICouponRepository couponRepository,
    IProductVariantRepository variantRepository,
    ICartRepository cartRepository) : ICouponEvaluator
{
    public async Task<CouponEvaluationResult> EvaluateAsync(
        string couponCode,
        IReadOnlyList<CartItemForEvaluation> items,
        decimal cartSubtotal,
        int? userId,
        string? sessionId = null,
        CancellationToken ct = default)
    {
        var normalizedCode = couponCode.Trim().ToUpperInvariant();

        // ── 1. Load coupon with applicability rules (AsNoTracking, via repo) ─
        var coupon = await couponRepository.GetByCodeWithRulesAsync(normalizedCode, ct);
        if (coupon is null || !coupon.IsActive)
            return Invalid(CouponNotFound, "Coupon not found or is no longer active.");

        // ── 2. Date window ───────────────────────────────────────────────────
        var now = DateTime.UtcNow;
        if (coupon.StartsAt.HasValue && coupon.StartsAt.Value > now)
            return Invalid(CouponNotStarted, "This coupon is not yet active.");

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value <= now)
            return Invalid(CouponExpired, "This coupon has expired.");

        // ── 3. Global usage limit ────────────────────────────────────────────
        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            return Invalid(CouponUsageLimitReached, "This coupon has reached its usage limit.");

        // ── 4. Per-identity usage limit ──────────────────────────────────────
        if (userId.HasValue)
        {
            // Authenticated: check CouponUsage table
            var userUsage = await couponRepository.GetUsageCountByUserAsync(coupon.Id, userId.Value, ct);
            if (userUsage >= coupon.PerUserLimit)
                return Invalid(CouponPerUserLimitReached, "You have already used this coupon the maximum number of times.");
        }
        else if (!string.IsNullOrWhiteSpace(sessionId))
        {
            // Guest: check whether this exact coupon code is already on their active cart.
            // CouponUsage is only written at checkout, so the cart row is the only signal available.
            var guestCart = await cartRepository.GetBySessionIdAsync(sessionId, ct);
            if (guestCart is not null &&
                string.Equals(guestCart.CouponCode, normalizedCode, StringComparison.OrdinalIgnoreCase))
                return Invalid(CouponAlreadyApplied, "This coupon is already applied to your cart.");
        }

        // ── 5. Applicability — resolve qualifying line-items via repos ────────
        var variantIds = items.Select(i => i.VariantId).Distinct().ToList();
        List<CartItemForEvaluation> qualifyingItems;

        if (coupon.ApplicableCategories.Count > 0)
        {
            var couponCategoryIds = coupon.ApplicableCategories.Select(c => c.CategoryId).ToList();
            var variantProductMap = await variantRepository.GetProductIdsByVariantIdsAsync(variantIds, ct);
            var productIds = variantProductMap.Values.Distinct().ToList();

            var qualifyingProductIds = await couponRepository.GetProductIdsInCategoriesAsync(
                couponCategoryIds, productIds, ct);

            qualifyingItems = items
                .Where(i => variantProductMap.TryGetValue(i.VariantId, out var pid) && qualifyingProductIds.Contains(pid))
                .ToList();
        }
        else if (coupon.ApplicableProducts.Count > 0)
        {
            var couponProductIds = coupon.ApplicableProducts.Select(p => p.ProductId).ToHashSet();
            var variantProductMap = await variantRepository.GetProductIdsByVariantIdsAsync(variantIds, ct);

            qualifyingItems = items
                .Where(i => variantProductMap.TryGetValue(i.VariantId, out var pid) && couponProductIds.Contains(pid))
                .ToList();
        }
        else if (coupon.ApplicableVariants.Count > 0)
        {
            var couponVariantIds = coupon.ApplicableVariants.Select(v => v.VariantId).ToHashSet();
            qualifyingItems = items.Where(i => couponVariantIds.Contains(i.VariantId)).ToList();
        }
        else
        {
            qualifyingItems = items.ToList();
        }

        if (qualifyingItems.Count == 0)
            return Invalid(CouponNotApplicable, "This coupon does not apply to any items in your cart.");

        // ── 6. Minimum order amount ──────────────────────────────────────────
        if (coupon.MinOrderAmount.HasValue && cartSubtotal < coupon.MinOrderAmount.Value)
            return Invalid(MinOrderAmountNotMet,
                $"Your cart total must be at least {coupon.MinOrderAmount.Value:F2} to use this coupon.");

        // ── 7. Calculate discount ────────────────────────────────────────────
        var qualifyingSubtotal = qualifyingItems.Sum(i => i.LineTotal);

        decimal discountAmount;
        if (coupon.DiscountType == DiscountType.Percentage)
        {
            var rawDiscount = qualifyingSubtotal * (coupon.DiscountValue / 100m);
            discountAmount = coupon.MaxDiscountAmount.HasValue
                ? Math.Min(rawDiscount, coupon.MaxDiscountAmount.Value)
                : rawDiscount;
        }
        else
        {
            discountAmount = Math.Min(coupon.DiscountValue, qualifyingSubtotal);
        }

        discountAmount = Math.Round(discountAmount, 2);

        var discountTypeStr = coupon.DiscountType == DiscountType.Percentage ? "percentage" : "fixed";
        var description = coupon.DiscountType == DiscountType.Percentage
            ? $"{coupon.DiscountValue}% off eligible items"
            : $"{coupon.DiscountValue:F2} off eligible items";

        return new CouponEvaluationResult(
            IsValid: true,
            FailureCode: null,
            FailureReason: null,
            DiscountAmount: discountAmount,
            CouponDto: new CartCouponDto(coupon.Code, discountTypeStr, discountAmount, description),
            CouponId: coupon.Id);
    }

    private static CouponEvaluationResult Invalid(BusinessRuleCode code, string reason) =>
        new(false, code, reason, 0m, null, null);
}
