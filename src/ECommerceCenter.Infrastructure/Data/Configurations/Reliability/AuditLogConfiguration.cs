using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Reliability;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.ActorType)
            .IsRequired()
            .HasMaxLength(16)
            .HasConversion<string>();

        entity.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(e => e.BeforeJson).HasColumnType("nvarchar(max)");
        entity.Property(e => e.AfterJson).HasColumnType("nvarchar(max)");

        entity.Property(e => e.CorrelationId).HasMaxLength(64);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => new { e.EntityType, e.EntityId, e.CreatedAt })
            .HasDatabaseName("IX_AuditLogs_Entity");

        entity.HasIndex(e => new { e.ActorId, e.CreatedAt })
            .HasDatabaseName("IX_AuditLogs_Actor");

        entity.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_AuditLogs_CreatedAt");
    }
}
