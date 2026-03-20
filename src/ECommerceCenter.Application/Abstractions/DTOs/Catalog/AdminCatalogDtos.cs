namespace ECommerceCenter.Application.Abstractions.DTOs.Catalog;

public record ChangeProductStatusBody(int Status);

public record BulkChangeProductStatusBody(List<int> Ids, int Status);

public record UpdateProductBody(
    string Title,
    string? Slug,
    string? Description,
    string? Brand);

public record AddVariantBody(
    Dictionary<string, string> Options,
    decimal BasePrice,
    string CurrencyCode,
    bool IsActive = true,
    int InitialStock = 0);

public record UpdateVariantBody(
    string Sku,
    Dictionary<string, string> Options,
    decimal BasePrice,
    string CurrencyCode,
    bool IsActive);

public record UpdateCategoryBody(
    string Name,
    string? Slug,
    string? Description,
    string? ImageUrl,
    int? ParentId,
    int SortOrder,
    bool IsActive,
    string? ExternalCategoryId = null,
    int? Supplier = null);

public record AdminInventoryDto(int OnHand, int Available);

// ── Products ───────────────────────────────────────────────────────────────────

public record AdminProductCreatedDto(
    int Id,
    string Title,
    string Slug,
    string? Description,
    string? Brand,
    int Status,
    DateTime CreatedAt);

public record AdminProductListItemDto(
    int Id,
    string Title,
    string Slug,
    string? Brand,
    int Status,
    int ActiveVariantCount,
    string? PrimaryImageUrl,
    DateTime CreatedAt);

public record AdminProductListFilter(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Search = null,
    string SortBy = "newest");

public record AdminProductVariantDto(
    int Id,
    string Sku,
    Dictionary<string, string> Options,
    decimal BasePrice,
    decimal? SupplierPrice,
    string CurrencyCode,
    bool IsActive,
    AdminInventoryDto Inventory);

public record AdminProductDetailDto(
    int Id,
    string Title,
    string Slug,
    string? Description,
    string? DescriptionHtml,
    string? Brand,
    int Status,
    decimal? WeightGrams,
    string? Material,
    ProductCategoryDto? Category,
    List<AdminProductVariantDto> Variants,
    List<ProductImageDto> Images,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// ── Variants ───────────────────────────────────────────────────────────────────

public record AdminVariantCreatedDto(
    int Id,
    int ProductId,
    string Sku,
    Dictionary<string, string> Options,
    decimal BasePrice,
    decimal? SupplierPrice,
    string CurrencyCode,
    bool IsActive,
    AdminInventoryDto Inventory);

// ── Categories ─────────────────────────────────────────────────────────────────

public record AdminCategoryDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    int? ParentId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? ExternalCategoryId = null,
    int? Supplier = null);

// ── SetProductCategory ────────────────────────────────────────────────────────────────

public record SetProductCategoryBody(int? CategoryId);
