using ECommerceCenter.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Inventory;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> entity)
    {
        // VariantId is both PK and FK — 1-to-1 with ProductVariant.
        entity.HasKey(e => e.VariantId);
        entity.Property(e => e.VariantId).ValueGeneratedNever();

        entity.Property(e => e.OnHand).IsRequired();

        // Maps to SQL Server rowversion — optimistic concurrency token.
        entity.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.ToTable(t =>
        {
            t.HasCheckConstraint("CK_InventoryItems_OnHand_NonNegative", "[OnHand] >= 0");
        });

        entity.HasOne(e => e.Variant)
            .WithOne(v => v.InventoryItem)
            .HasForeignKey<InventoryItem>(e => e.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
