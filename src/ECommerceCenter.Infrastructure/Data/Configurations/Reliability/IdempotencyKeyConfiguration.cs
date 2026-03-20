using ECommerceCenter.Domain.Entities.Reliability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Reliability;

public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.Route)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.RequestHash)
            .IsRequired()
            .HasMaxLength(128);

        entity.Property(e => e.ResponseJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // DEDUPE: same Idempotency-Key header + same route = one record.
        entity.HasIndex(e => new { e.Key, e.Route })
            .IsUnique()
            .HasDatabaseName("UX_IdempotencyKeys_Key_Route");

        entity.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_IdempotencyKeys_CreatedAt");

        entity.HasIndex(e => e.ExpiresAt)
            .HasDatabaseName("IX_IdempotencyKeys_ExpiresAt");
    }
}
