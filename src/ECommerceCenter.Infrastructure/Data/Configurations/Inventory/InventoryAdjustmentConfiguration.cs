using ECommerceCenter.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Inventory;

public class InventoryAdjustmentConfiguration : IEntityTypeConfiguration<InventoryAdjustment>
{
    public void Configure(EntityTypeBuilder<InventoryAdjustment> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => new { e.VariantId, e.CreatedAt })
            .HasDatabaseName("IX_InventoryAdjustments_VariantId_CreatedAt");

        entity.HasOne(e => e.Variant)
            .WithMany(v => v.Adjustments)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
