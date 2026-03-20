namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>
/// Projected variant data returned by <see cref="IProductVariantRepository.GetVariantDetailAsync"/>.
/// The handler maps this to <c>VariantDetailDto</c>.
/// </summary>
public record VariantDetailRow(
    int Id,
    string Sku,
    string? OptionsJson,
    decimal BasePrice,
    string CurrencyCode,
    int OnHand,
    List<CatalogImageRow> Images);
