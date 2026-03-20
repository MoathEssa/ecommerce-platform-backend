using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Catalog;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProductStatus Status { get; set; }
    public string? Brand { get; set; }

    // ── Supplier linkage (nullable — own products have null) ──────────────
    /// <summary>The supplier this product came from. Null = own product.</summary>
    public SupplierType? Supplier { get; set; }

    /// <summary>External supplier product ID (e.g. CJ product PID). Null = own product.</summary>
    public string? ExternalProductId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── Category FK (direct — a product belongs to at most one category) ──────
    public int? CategoryId { get; set; }

    // Navigation
    public Category? Category { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
}
