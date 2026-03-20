using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Domain.Entities.Coupons;

/// <summary>
/// Restricts a coupon to specific variants (SKU-level).
/// If no rows exist for a coupon, the coupon applies to all variants.
/// </summary>
public class CouponApplicableVariant
{
    public int CouponId { get; set; }
    public int VariantId { get; set; }

    // Navigation
    public Coupon Coupon { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
