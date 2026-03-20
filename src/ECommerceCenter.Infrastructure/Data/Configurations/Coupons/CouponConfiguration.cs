using ECommerceCenter.Domain.Entities.Coupons;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Coupons;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.DiscountType)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion<string>();

        entity.Property(e => e.DiscountValue)
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.MinOrderAmount)
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.MaxDiscountAmount)
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.PerUserLimit)
            .HasDefaultValue(1);

        entity.Property(e => e.UsedCount)
            .HasDefaultValue(0);

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("UX_Coupons_Code");

        // Worker/checkout scan: active, not expired coupons.
        entity.HasIndex(e => new { e.IsActive, e.ExpiresAt })
            .HasDatabaseName("IX_Coupons_IsActive_ExpiresAt");

        entity.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Coupons_DiscountValue_Positive", "[DiscountValue] > 0");
            t.HasCheckConstraint("CK_Coupons_PerUserLimit_Positive", "[PerUserLimit] > 0");
            t.HasCheckConstraint("CK_Coupons_UsedCount_NonNegative", "[UsedCount] >= 0");
        });
    }
}
