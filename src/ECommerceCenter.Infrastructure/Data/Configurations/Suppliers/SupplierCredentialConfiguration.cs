using ECommerceCenter.Domain.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Suppliers;

public class SupplierCredentialConfiguration : IEntityTypeConfiguration<SupplierCredential>
{
    public void Configure(EntityTypeBuilder<SupplierCredential> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.SupplierType)
            .IsRequired()
            .HasConversion<int>();

        entity.HasIndex(e => e.SupplierType)
            .IsUnique()
            .HasDatabaseName("UX_SupplierCredentials_SupplierType");

        entity.Property(e => e.ApiKey)
            .IsRequired()
            .HasMaxLength(300);

        entity.Property(e => e.AccessToken)
            .HasMaxLength(300);

        entity.Property(e => e.RefreshToken)
            .HasMaxLength(300);

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Computed helpers are not mapped to columns
        entity.Ignore(e => e.IsAccessTokenExpired);
        entity.Ignore(e => e.IsRefreshTokenExpired);
    }
}
