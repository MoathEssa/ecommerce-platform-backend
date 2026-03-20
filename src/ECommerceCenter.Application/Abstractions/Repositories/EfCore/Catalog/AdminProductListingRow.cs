namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>
/// Flat projection returned by <see cref="IProductRepository.GetAdminProductListingPagedAsync"/>.
/// The handler maps this to <c>AdminProductListItemDto</c>.
/// </summary>
public record AdminProductListingRow(
    int Id,
    string Title,
    string Slug,
    string? Brand,
    int Status,
    int ActiveVariantCount,
    string? CoverImageUrl,
    DateTime CreatedAt);
