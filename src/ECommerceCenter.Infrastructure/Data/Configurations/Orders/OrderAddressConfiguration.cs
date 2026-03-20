using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Orders;

public class OrderAddressConfiguration : IEntityTypeConfiguration<OrderAddress>
{
    public void Configure(EntityTypeBuilder<OrderAddress> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(16)
            .HasConversion<string>();

        entity.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.Phone).HasMaxLength(50);

        entity.Property(e => e.Line1)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.Line2).HasMaxLength(200);

        entity.Property(e => e.City)
            .IsRequired()
            .HasMaxLength(120);

        entity.Property(e => e.Region).HasMaxLength(120);
        entity.Property(e => e.PostalCode).HasMaxLength(30);

        entity.Property(e => e.Country)
            .IsRequired()
            .HasMaxLength(2);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.OrderId)
            .HasDatabaseName("IX_OrderAddresses_OrderId");

        entity.HasOne(e => e.Order)
            .WithMany(o => o.Addresses)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
