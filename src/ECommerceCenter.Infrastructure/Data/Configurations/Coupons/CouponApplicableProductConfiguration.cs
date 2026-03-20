using ECommerceCenter.Domain.Entities.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Coupons;

public class CouponApplicableProductConfiguration : IEntityTypeConfiguration<CouponApplicableProduct>
{
    public void Configure(EntityTypeBuilder<CouponApplicableProduct> entity)
    {
        entity.HasKey(e => new { e.CouponId, e.ProductId });

        entity.HasOne(e => e.Coupon)
            .WithMany(c => c.ApplicableProducts)
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
