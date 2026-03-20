using ECommerceCenter.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Catalog;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Sku)
            .HasMaxLength(64);

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.Property(e => e.OptionsJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        entity.Property(e => e.BasePrice)
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.SupplierPrice)
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasColumnType("char(3)");

        // ── Supplier linkage (nullable) ────────────────────────────────────────
        entity.Property(e => e.ExternalSkuId)
            .HasMaxLength(200);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.Sku)
            .IsUnique()
            .HasFilter("[Sku] IS NOT NULL")
            .HasDatabaseName("UX_ProductVariants_Sku");

        entity.HasIndex(e => e.ProductId)
            .HasDatabaseName("IX_ProductVariants_ProductId");

        entity.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_ProductVariants_IsActive");

        entity.HasOne(e => e.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
