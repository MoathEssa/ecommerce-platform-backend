using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Catalog;

/// <summary>
/// Product category node in a hierarchical tree.
/// ParentId = NULL means it is a root (top-level) category.
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly identifier, e.g. "mens-shoes". Unique across all categories.</summary>
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Banner / thumbnail image for the category page.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Controls display order among siblings.</summary>
    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    /// <summary>Self-referential FK — NULL = root category.</summary>
    public int? ParentId { get; set; }
    // ── Supplier linkage (nullable — own categories have null) ────────────
    /// <summary>The supplier this category was imported from (e.g. CjDropshipping). Null = own category.</summary>
    public SupplierType? Supplier { get; set; }

    /// <summary>The external supplier category ID (e.g. CJ leaf category GUID). Null = own category.</summary>
    public string? ExternalCategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
