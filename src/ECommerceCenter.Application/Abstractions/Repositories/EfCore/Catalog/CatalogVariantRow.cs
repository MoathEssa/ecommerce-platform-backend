namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>
/// Raw variant data projected for both storefront and admin views.
/// <c>IsActive</c> is included for the admin view; storefront callers can ignore it
/// (the repo filters to active-only before projecting).
/// </summary>
public record CatalogVariantRow(
    int Id,
    string Sku,
    string? OptionsJson,
    decimal BasePrice,
    decimal? SupplierPrice,
    string CurrencyCode,
    bool IsActive,
    int OnHand);
