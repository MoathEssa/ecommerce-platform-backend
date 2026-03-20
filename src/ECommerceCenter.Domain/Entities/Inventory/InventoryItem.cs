using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Domain.Entities.Inventory;

public class InventoryItem
{
    /// <summary>PK and FK to ProductVariants — 1-to-1 with ProductVariant.</summary>
    public int VariantId { get; set; }
    public int OnHand { get; set; }
    /// <summary>Optimistic concurrency token — mapped to rowversion in SQL Server.</summary>
    public byte[] RowVersion { get; set; } = [];
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ProductVariant Variant { get; set; } = null!;
}
