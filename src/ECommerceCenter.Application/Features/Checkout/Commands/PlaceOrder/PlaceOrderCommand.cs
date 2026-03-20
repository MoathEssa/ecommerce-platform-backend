using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Checkout.Commands.PlaceOrder;

// ── Request body (parsed by the API layer) ────────────────────────────────────

public record PlaceOrderRequestBody(
    string? Email,
    List<PlaceOrderItemBody> Items,
    PlaceOrderAddressBody ShippingAddress,
    PlaceOrderAddressBody? BillingAddress,
    string? CouponCode)
{
    /// <summary>Deterministic SHA-256 hex digest of the serialised request body.</summary>
    public string ComputeHash() =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this))))
               .ToLowerInvariant();
}

public record PlaceOrderItemBody(int VariantId, int Quantity)
{
    /// <summary>Projects the API body into the application-layer DTO.</summary>
    public CheckoutItemDto ToDto() => new(VariantId, Quantity);
}

public record PlaceOrderAddressBody(
    string FullName,
    string? Phone,
    string Line1,
    string? Line2,
    string City,
    string? Region,
    string? PostalCode,
    string Country)
{
    /// <summary>Projects the API body into the application-layer DTO.</summary>
    public CheckoutAddressDto ToDto() =>
        new(FullName, Phone, Line1, Line2, City, Region, PostalCode, Country);
}

// ── Application-layer command ─────────────────────────────────────────────────

public record CheckoutAddressDto(
    string FullName,
    string? Phone,
    string Line1,
    string? Line2,
    string City,
    string? Region,
    string? PostalCode,
    string Country);

public record CheckoutItemDto(int VariantId, int Quantity);

public record PlaceOrderCommand(
    int? UserId,
    string? Email,
    string IdempotencyKey,
    string RequestBodyHash,
    IReadOnlyList<CheckoutItemDto> Items,
    CheckoutAddressDto ShippingAddress,
    CheckoutAddressDto? BillingAddress,
    string? CouponCode) : IRequest<Result<PlaceOrderResponse>>;

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record PlaceOrderResponse(
    int OrderId,
    string OrderNumber,
    decimal Total,
    string CurrencyCode,
    string Status,
    PlaceOrderPaymentDto Payment,
    PlaceOrderSummaryDto Summary);

public record PlaceOrderPaymentDto(
    string Provider,
    string ClientSecret,
    string IntentId);

public record PlaceOrderSummaryDto(
    int ItemCount,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal ShippingTotal,
    decimal Total);

