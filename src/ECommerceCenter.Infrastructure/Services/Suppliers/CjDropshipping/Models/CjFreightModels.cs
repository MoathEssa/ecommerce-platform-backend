using System.Text.Json.Serialization;

namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping.Models;

// ── Outbound payload ─────────────────────────────────────────────────────────

internal sealed record CjFreightItemPayload(
    [property: JsonPropertyName("vid")] string Vid,
    [property: JsonPropertyName("quantity")] int Quantity);

internal sealed record CjFreightCalculatePayload(
    [property: JsonPropertyName("startCountryCode")] string StartCountryCode,
    [property: JsonPropertyName("endCountryCode")] string EndCountryCode,
    [property: JsonPropertyName("zip")] string? Zip,
    [property: JsonPropertyName("products")] List<CjFreightItemPayload> Products);

// ── Inbound response ─────────────────────────────────────────────────────────

internal sealed record CjFreightOption(
    [property: JsonPropertyName("logisticName")] string LogisticName,
    [property: JsonPropertyName("logisticPrice")] decimal LogisticPrice,
    [property: JsonPropertyName("logisticPriceCn")] decimal? LogisticPriceCn,
    [property: JsonPropertyName("logisticAging")] string LogisticAging,
    [property: JsonPropertyName("taxesFee")] decimal? TaxesFee,
    [property: JsonPropertyName("clearanceOperationFee")] decimal? ClearanceOperationFee,
    [property: JsonPropertyName("totalPostageFee")] decimal? TotalPostageFee);
