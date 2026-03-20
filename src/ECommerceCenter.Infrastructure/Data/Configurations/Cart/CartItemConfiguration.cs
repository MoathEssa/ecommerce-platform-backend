using ECommerceCenter.Domain.Entities.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Cart;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.ToTable(t => t.HasCheckConstraint("CK_CartItems_Qty_Positive", "[Quantity] > 0"));

        entity.HasIndex(e => e.CartId)
            .HasDatabaseName("IX_CartItems_CartId");

        // Each variant can appear at most once per cart.
        entity.HasIndex(e => new { e.CartId, e.VariantId })
            .IsUnique()
            .HasDatabaseName("UX_CartItems_Cart_Variant");

        entity.HasOne(e => e.Cart)
            .WithMany((CartEntity c) => c.Items)
            .HasForeignKey(e => e.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Variant)
            .WithMany(v => v.CartItems)
            .HasForeignKey(e => e.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
