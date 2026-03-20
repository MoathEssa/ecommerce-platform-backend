namespace ECommerceCenter.Application.Abstractions.Services.Suppliers;

// ── Flat DTO returned up to the application layer ─────────────────────────────

/// <summary>
/// Represents a single node in the CJ category hierarchy as returned by the
/// application layer. The hierarchy has exactly 3 levels:
/// <list type="bullet">
///   <item>Level 1 — top-level group. <see cref="CategoryId"/> is the CJ <c>categoryFirstId</c> UUID.</item>
///   <item>Level 2 — sub-group. <see cref="CategoryId"/> is the CJ <c>categorySecondId</c> UUID.</item>
///   <item>Level 3 — leaf category. <see cref="CategoryId"/> is the CJ <c>categoryId</c> UUID used
///     to query CJ products.</item>
/// </list>
/// </summary>
public record CjCategoryNodeDto(
    string? CategoryId,
    string Name,
    int Level,
    List<CjCategoryNodeDto> Children,
    bool IsImported = false);

// ── CJ Product DTOs ──────────────────────────────────────────────────────────

public record CjProductListItemDto(
    string Id,
    string NameEn,
    string? Sku,
    string? BigImage,
    string? SellPrice,
    string? DiscountPrice,
    string? DiscountPriceRate,
    int? ListedNum,
    string? CategoryId,
    string? ThreeCategoryName,
    string? TwoCategoryName,
    string? OneCategoryName,
    bool FreeShipping,
    string? ProductType,
    string? SupplierName,
    long? CreateAt,
    long? WarehouseInventoryNum,
    string? DeliveryCycle);

public record CjProductListResult(
    List<CjProductListItemDto> Items,
    int PageNumber,
    int PageSize,
    long TotalRecords,
    int TotalPages);

public record CjProductSearchParams(
    string? KeyWord = null,
    int Page = 1,
    int Size = 20,
    string? CategoryId = null,
    string? CountryCode = null,
    decimal? StartSellPrice = null,
    decimal? EndSellPrice = null,
    int? AddMarkStatus = null,
    int? ProductType = null,
    string? Sort = null,
    int? OrderBy = null);

// ── CJ Product Variant DTO ────────────────────────────────────────────────────

public record CjProductVariantDto(
    string Vid,
    string Pid,
    string? VariantNameEn,
    string? VariantImage,
    string? VariantSku,
    string? VariantKey,
    string? VariantStandard,
    decimal? VariantSellPrice,
    decimal? VariantSugSellPrice,
    decimal? VariantWeight);

// ── CJ Product Detail DTO (from /v1/product/query) ───────────────────────────

public record CjProductDetailVariantDto(
    string Vid,
    string? VariantNameEn,
    string? VariantImage,
    string? VariantSku,
    string? VariantKey,
    decimal? VariantSellPrice,
    decimal? VariantSugSellPrice,
    decimal? VariantWeight,
    int? LengthMm,
    int? WidthMm,
    int? HeightMm);

public record CjProductDetailDto(
    string Pid,
    string? ProductNameEn,
    string? ProductSku,
    List<string> ImageUrls,
    string? Description,
    decimal? WeightGrams,
    string? Material,
    string? ProductKeyEn,
    decimal? SellPrice,
    decimal? SuggestSellPrice,
    string? CategoryId,
    List<CjProductDetailVariantDto> Variants);

// ── CJ Review DTOs ─────────────────────────────────────────────────────────────────────────────

public record CjReviewItemDto(
    long CommentId,
    string? CommentUser,
    int Score,
    string Comment,
    string? CommentDate,
    string? CountryCode,
    string? FlagIconUrl,
    List<string> CommentUrls);

public record CjReviewListDto(
    int Total,
    List<CjReviewItemDto> Items);

/// <summary>
/// Service for browsing the CJDropshipping product catalogue.
/// </summary>
public interface ICjProductService
{
    Task<List<CjCategoryNodeDto>> GetCategoriesAsync(CancellationToken ct = default);

    Task<CjProductListResult> SearchProductsAsync(
        CjProductSearchParams searchParams,
        CancellationToken ct = default);

    Task<List<CjProductVariantDto>> GetProductVariantsAsync(
        string pid,
        string? countryCode = null,
        CancellationToken ct = default);

    Task<CjProductDetailDto?> GetProductDetailAsync(
        string pid,
        CancellationToken ct = default);

    /// <summary>
    /// Queries the CJ stock API for a single variant.
    /// Returns the total stock across all warehouses, or null on failure.
    /// </summary>
    Task<int?> GetVariantStockAsync(string vid, CancellationToken ct = default);

    /// <summary>
    /// Fetches product reviews from CJ, filtered to English-speaking countries.
    /// </summary>
    Task<CjReviewListDto> GetProductReviewsAsync(
        string pid, int page, int pageSize, CancellationToken ct = default);
}
