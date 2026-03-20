namespace ECommerceCenter.Application.Common.Settings;

/// <summary>
/// Store-wide operational defaults.
/// Configure via appsettings: "Store": { "CurrencyCode": "SAR", "CountryTaxRates": { "SA": 0.15 } }
/// </summary>
public class StoreSettings
{
    public const string SectionName = "Store";

    /// <summary>ISO 4217 currency code used store-wide. Defaults to SAR.</summary>
    public string CurrencyCode { get; set; } = "SAR";

    /// <summary>
    /// VAT / sales-tax rates keyed by ISO 3166-1 alpha-2 country code (upper-case).
    /// Countries not listed default to 0 % tax.
    /// Example: <c>"SA": 0.15</c> applies 15 % VAT for Saudi Arabia.
    /// </summary>
    public Dictionary<string, decimal> CountryTaxRates { get; set; } = new() { ["SA"] = 0.15m };

    /// <summary>Returns the tax rate for <paramref name="countryCode"/>, or 0 if not configured.</summary>
    public decimal GetTaxRate(string countryCode) =>
        CountryTaxRates.TryGetValue(countryCode.ToUpperInvariant(), out var rate) ? rate : 0m;

    /// <summary>
    /// Maximum quantity allowed for a single order line (after aggregating duplicate variant rows).
    /// Also used as the upper bound in the FluentValidation per-item rule.
    /// </summary>
    public int MaxQuantityPerLine { get; set; } = 99;
}
