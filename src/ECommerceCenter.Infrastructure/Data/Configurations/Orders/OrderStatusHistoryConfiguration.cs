using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Orders;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.FromStatus)
            .IsRequired(false)
            .HasMaxLength(32)
            .HasConversion<string>();

        entity.Property(e => e.ToStatus)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion<string>();

        entity.Property(e => e.ChangedByType)
            .IsRequired()
            .HasMaxLength(16)
            .HasConversion<string>();

        entity.Property(e => e.Note).HasMaxLength(500);

        entity.Property(e => e.ChangedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => new { e.OrderId, e.ChangedAt })
            .HasDatabaseName("IX_OrderStatusHistory_OrderId_ChangedAt");

        entity.HasOne(e => e.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
