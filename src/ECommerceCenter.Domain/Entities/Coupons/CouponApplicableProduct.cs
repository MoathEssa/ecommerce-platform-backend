using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Domain.Entities.Coupons;

/// <summary>
/// Restricts a coupon to specific products.
/// If no rows exist for a coupon, the coupon applies to all products.
/// </summary>
public class CouponApplicableProduct
{
    public int CouponId { get; set; }
    public int ProductId { get; set; }

    // Navigation
    public Coupon Coupon { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
