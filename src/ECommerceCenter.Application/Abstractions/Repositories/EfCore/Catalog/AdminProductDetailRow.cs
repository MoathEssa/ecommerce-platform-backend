namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>
/// Projected product data returned by <see cref="IProductRepository.GetAdminProductDetailByIdAsync"/>.
/// The handler maps this to <c>AdminProductDetailDto</c>.
/// </summary>
public record AdminProductDetailRow(
    int Id,
    string Title,
    string Slug,
    string? Description,
    string? Brand,
    int Status,
    string? ExternalProductId,
    int? Supplier,
    CatalogCategoryRefRow? Category,
    List<CatalogVariantRow> Variants,
    List<CatalogImageRow> Images,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
