using ECommerceCenter.Domain.Entities.Orders;

namespace ECommerceCenter.Domain.Entities.Coupons;

/// <summary>
/// Records that a coupon was applied to a specific order.
/// Enforces global UsageLimit and PerUserLimit at query time.
/// </summary>
public class CouponUsage
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public int OrderId { get; set; }
    /// <summary>Null for guest checkouts.</summary>
    public int? UserId { get; set; }
    /// <summary>Actual discount amount applied to this order (computed and stored for auditability).</summary>
    public decimal DiscountApplied { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Coupon Coupon { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
