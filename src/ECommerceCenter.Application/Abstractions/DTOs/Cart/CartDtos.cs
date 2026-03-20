namespace ECommerceCenter.Application.Abstractions.DTOs.Cart;

// ── Request body DTOs (kept separate from MediatR commands) ─────────────────

public record AddCartItemBody(int VariantId, int Quantity);
public record UpdateCartItemBody(int Quantity);
public record ApplyCouponBody(string Code);

// ── Response DTOs ────────────────────────────────────────────────────────────

public record CartItemDto(
    int Id,
    int VariantId,
    int ProductId,
    string ProductTitle,
    string ProductSlug,
    string Sku,
    Dictionary<string, string> Options,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    string StockStatus,
    List<string> Warnings);

public record CartCouponDto(
    string Code,
    string DiscountType,
    decimal DiscountAmount,
    string? Description);

public record CartDto(
    int Id,
    string CurrencyCode,
    List<CartItemDto> Items,
    CartCouponDto? Coupon,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal Total,
    int ItemCount);

public record AdminCartListItemDto(
    int CartId,
    int? UserId,
    string? UserEmail,
    string? UserName,
    string? SessionId,
    int ItemCount,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal Total,
    string CurrencyCode,
    string? CouponCode,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
