using ECommerceCenter.Domain.Entities.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Coupons;

public class CouponApplicableVariantConfiguration : IEntityTypeConfiguration<CouponApplicableVariant>
{
    public void Configure(EntityTypeBuilder<CouponApplicableVariant> entity)
    {
        entity.HasKey(e => new { e.CouponId, e.VariantId });

        entity.HasOne(e => e.Coupon)
            .WithMany(c => c.ApplicableVariants)
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Variant)
            .WithMany()
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
