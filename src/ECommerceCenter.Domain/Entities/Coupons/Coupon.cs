using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Coupons;

public class Coupon
{
    public int Id { get; set; }

    /// <summary>The code customers type at checkout. Stored as snapshot in Orders.CouponCode.</summary>
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// For Percentage: 0–100. For FixedAmount: amount in order currency.
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>Minimum order subtotal required to apply this coupon. Null = no minimum.</summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Maximum discount amount for Percentage coupons (cap). Null = no cap.
    /// Ignored for FixedAmount.
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>Total number of times this coupon can be used globally. Null = unlimited.</summary>
    public int? UsageLimit { get; set; }

    /// <summary>How many times a single user can use this coupon.</summary>
    public int PerUserLimit { get; set; } = 1;

    /// <summary>Current global usage count. Incremented on each successful use.</summary>
    public int UsedCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    /// <summary>
    /// Specific categories this coupon is restricted to.
    /// Empty = applies to all categories.
    /// Checked before product/variant restrictions (coarser scope first).
    /// </summary>
    public ICollection<CouponApplicableCategory> ApplicableCategories { get; set; } = [];

    /// <summary>
    /// Specific products this coupon is restricted to.
    /// Empty = applies to all products (within category restrictions above).
    /// </summary>
    public ICollection<CouponApplicableProduct> ApplicableProducts { get; set; } = [];

    /// <summary>
    /// Specific variants this coupon is restricted to.
    /// Empty = applies to all variants (within product restrictions above).
    /// </summary>
    public ICollection<CouponApplicableVariant> ApplicableVariants { get; set; } = [];

    public ICollection<CouponUsage> Usages { get; set; } = [];
}
