using ECommerceCenter.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Catalog;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Description)
            .HasColumnType("nvarchar(max)");

        entity.Property(e => e.ImageUrl).HasMaxLength(500);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");
        // ── Supplier linkage ──────────────────────────────────────────────
        entity.Property(e => e.Supplier);

        entity.Property(e => e.ExternalCategoryId)
            .HasMaxLength(200);

        // Unique per supplier — prevents importing the same CJ category twice
        entity.HasIndex(e => new { e.Supplier, e.ExternalCategoryId })
            .HasFilter("[Supplier] IS NOT NULL AND [ExternalCategoryId] IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("UX_Categories_Supplier_ExternalCategoryId");
        // ── Indexes ───────────────────────────────────────────────────────
        entity.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("UX_Categories_Slug");

        entity.HasIndex(e => e.ParentId)
            .HasDatabaseName("IX_Categories_ParentId");

        entity.HasIndex(e => new { e.IsActive, e.SortOrder })
            .HasDatabaseName("IX_Categories_IsActive_SortOrder");

        // ── Self-referential hierarchy ────────────────────────────────────
        entity.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // prevent cascade-delete of entire subtrees
    }
}
