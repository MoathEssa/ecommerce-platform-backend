using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Payments;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(e => e.ProviderIntentId)
            .IsRequired()
            .HasMaxLength(128);

        entity.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion<string>();

        entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        entity.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasColumnType("char(3)");

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.OrderId)
            .HasDatabaseName("IX_PaymentAttempts_OrderId");

        entity.HasIndex(e => new { e.Provider, e.ProviderIntentId })
            .IsUnique()
            .HasDatabaseName("UX_PaymentAttempts_Provider_Intent");

        entity.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_PaymentAttempts_Status_CreatedAt");

        entity.HasOne(e => e.Order)
            .WithMany(o => o.PaymentAttempts)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
