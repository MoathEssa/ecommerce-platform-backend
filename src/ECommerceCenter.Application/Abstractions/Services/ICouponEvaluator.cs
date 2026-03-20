using ECommerceCenter.Application.Abstractions.DTOs.Cart;

namespace ECommerceCenter.Application.Abstractions.Services;

/// <summary>
/// Single authority for all coupon business rules.
/// Both <c>ApplyCouponCommandHandler</c> and <c>CartReadService</c> delegate to this service
/// so evaluation logic lives in exactly one place.
/// </summary>
public interface ICouponEvaluator
{
    /// <param name="couponCode">Raw coupon code entered by the user (will be normalised internally).</param>
    /// <param name="items">Cart line-items with their current line totals.</param>
    /// <param name="cartSubtotal">Sum of all item line totals (before any discount).</param>
    /// <param name="userId">Null for guest carts; used to check per-user usage limits via <c>CouponUsage</c>.</param>
    /// <param name="sessionId">
    /// Guest session id. When <paramref name="userId"/> is null and this is provided,
    /// the current guest cart state is used to enforce the <c>PerUserLimit</c> per session.
    /// </param>
    Task<CouponEvaluationResult> EvaluateAsync(
        string couponCode,
        IReadOnlyList<CartItemForEvaluation> items,
        decimal cartSubtotal,
        int? userId,
        string? sessionId = null,
        CancellationToken ct = default);
}
