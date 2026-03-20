namespace ECommerceCenter.Application.Common.Constants;

/// <summary>
/// Storefront stock-status display thresholds.
/// Change these values in one place to affect all status mappings.
/// </summary>
public static class StockThresholds
{
    /// <summary>Available units >= this value → "inStock".</summary>
    public const int InStock = 10;

    /// <summary>Available units >= this value (and below InStock) → "lowStock".</summary>
    public const int LowStock = 1;

    public static string Map(int available) => available switch
    {
        >= InStock  => StockStatus.InStock,
        >= LowStock => StockStatus.LowStock,
        _           => StockStatus.OutOfStock
    };
}

/// <summary>String constants for the storefront stockStatus field.</summary>
public static class StockStatus
{
    public const string InStock    = "inStock";
    public const string LowStock   = "lowStock";
    public const string OutOfStock = "outOfStock";
}
