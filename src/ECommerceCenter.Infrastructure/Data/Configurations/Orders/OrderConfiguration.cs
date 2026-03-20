using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(40);

        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        entity.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion<string>();

        entity.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasColumnType("char(3)");

        entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
        entity.Property(e => e.DiscountTotal).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        entity.Property(e => e.TaxTotal).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        entity.Property(e => e.ShippingTotal).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

        entity.Property(e => e.CouponCode).HasMaxLength(64);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.OrderNumber)
            .IsUnique()
            .HasDatabaseName("UX_Orders_OrderNumber");

        entity.HasIndex(e => new { e.UserId, e.CreatedAt })
            .HasDatabaseName("IX_Orders_UserId_CreatedAt");

        entity.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_Orders_Status_CreatedAt");

        entity.HasIndex(e => new { e.Email, e.CreatedAt })
            .HasDatabaseName("IX_Orders_Email_CreatedAt");

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_Orders_Totals_NonNegative",
            "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [ShippingTotal] >= 0 AND [Total] >= 0"));
    }
}
