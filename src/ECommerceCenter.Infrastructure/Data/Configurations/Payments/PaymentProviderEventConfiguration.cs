using ECommerceCenter.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Payments;

public class PaymentProviderEventConfiguration : IEntityTypeConfiguration<PaymentProviderEvent>
{
    public void Configure(EntityTypeBuilder<PaymentProviderEvent> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(e => e.EventId)
            .IsRequired()
            .HasMaxLength(128);

        entity.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(128);

        entity.Property(e => e.RelatedIntentId).HasMaxLength(128);

        entity.Property(e => e.PayloadJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        entity.Property(e => e.ReceivedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // DEDUPE index — insert will fail on duplicate (Provider, EventId).
        entity.HasIndex(e => new { e.Provider, e.EventId })
            .IsUnique()
            .HasDatabaseName("UX_PaymentProviderEvents_Provider_EventId");

        entity.HasIndex(e => e.ReceivedAt)
            .HasDatabaseName("IX_PaymentProviderEvents_ReceivedAt");

        entity.HasIndex(e => e.RelatedIntentId)
            .HasDatabaseName("IX_PaymentProviderEvents_RelatedIntentId");
    }
}
