using ECommerceCenter.Domain.Entities.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Coupons;

public class CouponApplicableCategoryConfiguration : IEntityTypeConfiguration<CouponApplicableCategory>
{
    public void Configure(EntityTypeBuilder<CouponApplicableCategory> entity)
    {
        entity.HasKey(e => new { e.CouponId, e.CategoryId });

        entity.HasOne(e => e.Coupon)
            .WithMany(c => c.ApplicableCategories)
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
