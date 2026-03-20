using ECommerceCenter.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Orders;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.SkuSnapshot)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.NameSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.OrderId)
            .HasDatabaseName("IX_OrderItems_OrderId");

        entity.HasIndex(e => e.VariantId)
            .HasDatabaseName("IX_OrderItems_VariantId");

        entity.ToTable(t =>
        {
            t.HasCheckConstraint("CK_OrderItems_Qty_Positive", "[Quantity] > 0");
            t.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "[UnitPrice] >= 0");
            t.HasCheckConstraint("CK_OrderItems_LineTotal_NonNegative", "[LineTotal] >= 0");
        });

        entity.HasOne(e => e.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: snapshot must not vanish even if variant is deactivated.
        entity.HasOne(e => e.Variant)
            .WithMany(v => v.OrderItems)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
