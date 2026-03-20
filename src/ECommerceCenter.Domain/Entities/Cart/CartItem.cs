using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Domain.Entities.Cart;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int VariantId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Cart Cart { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
