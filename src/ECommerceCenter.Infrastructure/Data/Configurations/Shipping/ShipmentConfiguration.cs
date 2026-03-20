using ECommerceCenter.Domain.Entities.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Shipping;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Carrier)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.TrackingNumber)
            .IsRequired()
            .HasMaxLength(128);

        entity.Property(e => e.ShippedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => new { e.Carrier, e.TrackingNumber })
            .IsUnique()
            .HasDatabaseName("UX_Shipments_Tracking");

        entity.HasIndex(e => e.OrderId)
            .HasDatabaseName("IX_Shipments_OrderId");

        entity.HasOne(e => e.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
