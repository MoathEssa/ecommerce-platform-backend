using ECommerceCenter.Domain.Entities.Reliability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Reliability;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.PayloadJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        entity.Property(e => e.OccurredAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(e => e.Attempts).HasDefaultValue(0);

        entity.Property(e => e.LastError).HasMaxLength(2000);

        // Worker scan: WHERE ProcessedAt IS NULL (picks up pending messages).
        entity.HasIndex(e => e.ProcessedAt)
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt");

        entity.HasIndex(e => e.OccurredAt)
            .HasDatabaseName("IX_OutboxMessages_OccurredAt");
    }
}
