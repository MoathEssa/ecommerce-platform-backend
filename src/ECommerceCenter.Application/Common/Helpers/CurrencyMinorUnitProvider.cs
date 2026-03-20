namespace ECommerceCenter.Application.Common.Helpers;

/// <summary>
/// Returns the multiplier required to convert a decimal monetary amount into the
/// ISO 4217 smallest unit for a given currency code (e.g. SAR 1.00 → 100 halalas).
/// Used when constructing payment-provider amounts that must be expressed as integers.
/// </summary>
public static class CurrencyMinorUnitProvider
{
    // ISO 4217 exponent override table — currencies that differ from the standard 2 decimals.
    // Zero-decimal: JPY, KWD uses 3 but group it separately.
    private static readonly HashSet<string> ZeroDecimal = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIF", "CLP", "DJF", "GNF", "ISK", "JPY", "KMF", "KRW",
        "MGA", "PYG", "RWF", "UGX", "UYI", "VND", "VUV", "XAF", "XOF", "XPF"
    };

    private static readonly HashSet<string> ThreeDecimal = new(StringComparer.OrdinalIgnoreCase)
    {
        "BHD", "IQD", "JOD", "KWD", "LYD", "OMR", "TND"
    };

    /// <summary>
    /// Returns the integer multiplier for the currency (100 for standard, 1 for zero-decimal,
    /// 1000 for three-decimal).
    /// </summary>
    public static long GetMultiplier(string currencyCode) =>
        ZeroDecimal.Contains(currencyCode) ? 1L :
        ThreeDecimal.Contains(currencyCode) ? 1_000L :
        100L;

    /// <summary>
    /// Converts <paramref name="amount"/> to the currency's smallest unit, rounding half-up.
    /// </summary>
    public static long ToSmallestUnit(decimal amount, string currencyCode) =>
        (long)Math.Round(amount * GetMultiplier(currencyCode), MidpointRounding.AwayFromZero);
}
