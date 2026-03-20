using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Cart;

public class CartConfiguration : IEntityTypeConfiguration<CartEntity>
{
    public void Configure(EntityTypeBuilder<CartEntity> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasColumnType("char(3)");

        entity.Property(e => e.CouponCode).HasMaxLength(64);

        entity.Property(e => e.SessionId).HasMaxLength(64);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_Carts_UserId");

        entity.HasIndex(e => e.SessionId)
            .HasDatabaseName("IX_Carts_SessionId");
    }
}
