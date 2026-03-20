namespace ECommerceCenter.Application.Abstractions.DTOs.Catalog;

// ── Shared ──────────────────────────────────────────────────────────────────
public record BreadcrumbDto(string Name, string Slug);

/// <summary>Minimal category projection for in-memory tree building and breadcrumb walking.</summary>
public record CategorySummaryDto(int Id, string Name, string Slug, string? ImageUrl, string? Description, int? ParentId);

// ── Categories ──────────────────────────────────────────────────────────────
public record CategoryTreeDto(
    int Id,
    string Name,
    string Slug,
    string? ImageUrl,
    int SortOrder,
    List<CategoryTreeDto> Children);

public record CategoryChildDto(int Id, string Name, string Slug, string? ImageUrl);

public record CategoryDetailDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    List<BreadcrumbDto> Breadcrumbs,
    List<CategoryChildDto> Children);

// ── Products (Listing) ──────────────────────────────────────────────────────
public record ProductListCategoryDto(string Name, string Slug);

public record ProductListItemDto(
    int Id,
    string Title,
    string Slug,
    string? Brand,
    string? PrimaryImageUrl,
    decimal MinPrice,
    decimal MaxPrice,
    string CurrencyCode,
    bool InStock,
    ProductListCategoryDto? PrimaryCategory);

public record ProductListFilter(
    int Page = 1,
    int PageSize = 20,
    string? CategorySlug = null,
    string? Search = null,
    string? Brand = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string SortBy = "relevance",
    bool? InStock = null);

// ── Product Detail ──────────────────────────────────────────────────────────
public record ProductCategoryDto(int Id, string Name, string Slug);

public record ProductVariantDto(
    int Id,
    string Sku,
    Dictionary<string, string> Options,
    decimal BasePrice,
    string CurrencyCode,
    string StockStatus,
    string? VariantName,
    string? VariantImage);

public record ProductImageDto(int Id, string Url, int? VariantId, int SortOrder);

public record ProductDetailDto(
    int Id,
    string Title,
    string Slug,
    string? Description,
    string? DescriptionHtml,
    string? Brand,
    int Status,
    decimal? WeightGrams,
    string? Material,
    List<BreadcrumbDto> Breadcrumbs,
    ProductCategoryDto? Category,
    List<ProductVariantDto> Variants,
    List<ProductImageDto> Images,
    DateTime CreatedAt);

// ── Variant Detail ──────────────────────────────────────────────────────────
public record VariantDetailDto(
    int Id,
    string Sku,
    Dictionary<string, string> Options,
    decimal BasePrice,
    string CurrencyCode,
    string StockStatus,
    List<ProductImageDto> Images);

// ── Search Suggestions ──────────────────────────────────────────────────────
public record SearchSuggestionDto(string Type, string Title, string Slug);
// ── Product Reviews ───────────────────────────────────────────────────────────
public record ReviewDto(
    long CommentId,
    string? CommentUser,
    int Score,
    string Comment,
    string? CommentDate,
    string? CountryCode,
    string? FlagIconUrl,
    List<string> CommentUrls);

public record ReviewListDto(
    int PageNum,
    int PageSize,
    int Total,
    double AverageScore,
    List<ReviewDto> Items);