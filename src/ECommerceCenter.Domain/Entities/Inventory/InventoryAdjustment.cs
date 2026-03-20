using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Domain.Entities.Inventory;

public class InventoryAdjustment
{
    public int Id { get; set; }
    public int VariantId { get; set; }
    /// <summary>Positive or negative stock delta.</summary>
    public int Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
    /// <summary>Admin user Id who made the adjustment.</summary>
    public int? ActorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ProductVariant Variant { get; set; } = null!;
}
