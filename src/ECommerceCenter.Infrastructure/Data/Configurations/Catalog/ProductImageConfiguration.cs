using ECommerceCenter.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Catalog;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(e => e.SortOrder)
            .HasDefaultValue(0);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => new { e.ProductId, e.SortOrder })
            .HasDatabaseName("IX_ProductImages_ProductId");

        entity.HasIndex(e => e.VariantId)
            .HasDatabaseName("IX_ProductImages_VariantId");

        entity.HasOne(e => e.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ClientSetNull avoids a multiple-cascade-path issue:
        // Product→ProductVariant (cascade) and ProductVariant→ProductImage would
        // create two cascade paths from Product to ProductImage.
        entity.HasOne(e => e.Variant)
            .WithMany(v => v.Images)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
