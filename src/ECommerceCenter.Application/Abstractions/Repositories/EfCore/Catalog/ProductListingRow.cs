namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>
/// Flat projection returned by <see cref="IProductRepository.GetProductListingPagedAsync"/>.
/// The handler maps this to <c>ProductListItemDto</c>.
/// </summary>
public record ProductListingRow(
    int Id,
    string Title,
    string Slug,
    string? Brand,
    string? CoverImageUrl,
    decimal MinPrice,
    decimal MaxPrice,
    string CurrencyCode,
    bool HasStock,
    int? PrimaryCategoryId,
    string? PrimaryCategoryName,
    string? PrimaryCategorySlug);
