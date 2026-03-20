using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Catalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.Description)
            .HasColumnType("nvarchar(max)");

        entity.Property(e => e.Status)
            .IsRequired();

        entity.Property(e => e.Brand)
            .HasMaxLength(120);

        // ── Supplier linkage (nullable) ────────────────────────────────────────
        entity.Property(e => e.Supplier);

        entity.Property(e => e.ExternalProductId)
            .HasMaxLength(200);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("UX_Products_Slug");

        entity.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Products_Status");

        // ── Category FK (nullable) ────────────────────────────────────────────────
        entity.HasOne(e => e.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
