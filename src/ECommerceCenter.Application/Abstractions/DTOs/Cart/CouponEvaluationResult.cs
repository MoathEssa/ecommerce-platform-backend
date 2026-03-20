using ECommerceCenter.Application.Common.Errors;

namespace ECommerceCenter.Application.Abstractions.DTOs.Cart;

/// <summary>
/// Slim item descriptor passed to <see cref="Services.ICouponEvaluator"/>.
/// The evaluator resolves product / category IDs internally.
/// </summary>
public record CartItemForEvaluation(int VariantId, decimal LineTotal);

/// <summary>
/// Result returned by <see cref="Services.ICouponEvaluator.EvaluateAsync"/>.
/// </summary>
public record CouponEvaluationResult(
    bool IsValid,
    BusinessRuleCode? FailureCode,
    string? FailureReason,
    decimal DiscountAmount,
    CartCouponDto? CouponDto,
    /// <summary>Database PK of the matched coupon. Null when <see cref="IsValid"/> is false.</summary>
    int? CouponId);
