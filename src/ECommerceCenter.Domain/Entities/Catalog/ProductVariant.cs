using ECommerceCenter.Domain.Entities.Inventory;
using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Entities.Cart;

namespace ECommerceCenter.Domain.Entities.Catalog;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? Sku { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>JSON e.g. {"size":"M","color":"Black"}</summary>
    public string OptionsJson { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    /// <summary>Original supplier cost price (e.g. CJ sell price). Stored for margin calculations.</summary>
    public decimal? SupplierPrice { get; set; }
    /// <summary>ISO 4217 e.g. SAR</summary>
    public string CurrencyCode { get; set; } = string.Empty;
    // ── Supplier linkage (nullable — own variants have null) ────────────
    /// <summary>External supplier variant/SKU ID (e.g. CJ variant VID). Null = own variant.</summary>
    public string? ExternalSkuId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public InventoryItem? InventoryItem { get; set; }
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<InventoryAdjustment> Adjustments { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
