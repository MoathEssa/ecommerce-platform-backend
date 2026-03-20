using ECommerceCenter.Domain.Entities.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Coupons;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.DiscountApplied)
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.UsedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // One coupon usage per order — an order can only benefit from one coupon.
        entity.HasIndex(e => e.OrderId)
            .IsUnique()
            .HasDatabaseName("UX_CouponUsages_OrderId");

        // Per-user limit queries: count usages by (CouponId, UserId).
        entity.HasIndex(e => new { e.CouponId, e.UserId })
            .HasDatabaseName("IX_CouponUsages_CouponId_UserId");

        entity.ToTable(t =>
            t.HasCheckConstraint("CK_CouponUsages_DiscountApplied_NonNegative", "[DiscountApplied] >= 0"));

        entity.HasOne(e => e.Coupon)
            .WithMany(c => c.Usages)
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Order)
            .WithOne(o => o.CouponUsage)
            .HasForeignKey<CouponUsage>(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
