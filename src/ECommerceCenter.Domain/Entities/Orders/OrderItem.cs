using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Domain.Entities.Orders;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    /// <summary>Kept for historical reference even if variant is later deactivated.</summary>
    public int VariantId { get; set; }
    public string SkuSnapshot { get; set; } = string.Empty;
    public string NameSnapshot { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
