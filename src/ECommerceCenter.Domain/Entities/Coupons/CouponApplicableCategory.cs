using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Domain.Entities.Coupons;

/// <summary>
/// Restricts a coupon to products that belong to specific categories (primary or any).
/// If no rows exist for a coupon, the coupon applies to all categories.
/// Category-level applicability is checked by joining the product's categories
/// against this list at discount calculation time (app layer).
/// </summary>
public class CouponApplicableCategory
{
    public int CouponId { get; set; }
    public int CategoryId { get; set; }

    // Navigation
    public Coupon Coupon { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
