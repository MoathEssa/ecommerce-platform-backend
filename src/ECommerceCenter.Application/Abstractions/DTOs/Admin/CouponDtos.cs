namespace ECommerceCenter.Application.Abstractions.DTOs.Admin;

public record CouponListItemDto(
    int Id,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int PerUserLimit,
    int UsedCount,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    DateTime CreatedAt);

public record CouponDetailDto(
    int Id,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int PerUserLimit,
    int UsedCount,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    IReadOnlyList<CouponCategoryRefDto> ApplicableCategories,
    IReadOnlyList<CouponProductRefDto> ApplicableProducts,
    IReadOnlyList<CouponVariantRefDto> ApplicableVariants,
    CouponUsageStatsDto? UsageStats,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CouponCategoryRefDto(int Id, string Name);
public record CouponProductRefDto(int Id, string Title);
public record CouponVariantRefDto(int Id, string Sku);

public record CouponUsageStatsDto(
    int TotalUsed,
    int UniqueUsers,
    decimal TotalDiscountGiven,
    IReadOnlyList<CouponUsageItemDto> RecentUsages);

public record CouponUsageItemDto(
    int OrderId,
    string OrderNumber,
    int? UserId,
    string? Email,
    decimal DiscountApplied,
    DateTime CreatedAt);

public record CreateCouponBody(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int PerUserLimit,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    int[] ApplicableCategories,
    int[] ApplicableProducts,
    int[] ApplicableVariants);

public record UpdateCouponBody(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int PerUserLimit,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    int[] ApplicableCategories,
    int[] ApplicableProducts,
    int[] ApplicableVariants);
