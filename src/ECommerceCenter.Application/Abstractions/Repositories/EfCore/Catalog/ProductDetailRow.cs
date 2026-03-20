namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>
/// Projected product data returned by <see cref="IProductRepository.GetProductDetailBySlugAsync"/>.
/// The handler maps this (plus breadcrumbs it builds itself) to <c>ProductDetailDto</c>.
/// </summary>
public record ProductDetailRow(
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
    DateTime CreatedAt);
