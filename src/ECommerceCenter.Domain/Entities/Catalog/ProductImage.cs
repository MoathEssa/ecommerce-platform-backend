namespace ECommerceCenter.Domain.Entities.Catalog;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}
