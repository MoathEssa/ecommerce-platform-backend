using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Payments;

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(e => e.ProviderRefundId).HasMaxLength(128);

        entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        entity.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasColumnType("char(3)");

        entity.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion<string>();

        entity.Property(e => e.Reason).HasMaxLength(200);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => new { e.OrderId, e.CreatedAt })
            .HasDatabaseName("IX_Refunds_OrderId_CreatedAt");

        // Partial unique index — only when ProviderRefundId is not null.
        entity.HasIndex(e => new { e.Provider, e.ProviderRefundId })
            .IsUnique()
            .HasFilter("[ProviderRefundId] IS NOT NULL")
            .HasDatabaseName("UX_Refunds_Provider_RefundId");

        entity.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Refunds_Status");

        entity.HasOne(e => e.Order)
            .WithMany(o => o.Refunds)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // ClientSetNull: avoids multiple-cascade-path from Order→PaymentAttempt→Refund
        // while Order→Refund already has CASCADE.
        entity.HasOne(e => e.PaymentAttempt)
            .WithMany(p => p.Refunds)
            .HasForeignKey(e => e.PaymentAttemptId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
