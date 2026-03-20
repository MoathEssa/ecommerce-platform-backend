namespace ECommerceCenter.Application.Abstractions.DTOs.Payments;

public record StripeAddressDto(
    string? City,
    string? Country,
    string? Line1,
    string? Line2,
    string? PostalCode,
    string? State);

public record StripeBillingDetailsDto(
    StripeAddressDto? Address,
    string? Email,
    string? Name,
    string? Phone);

public record StripeCardDetailsDto(
    string? Brand,
    string? Country,
    int? ExpMonth,
    int? ExpYear,
    string? Fingerprint,
    string? Funding,
    string? Last4,
    string? Network);

public record StripePaymentMethodDetailsDto(
    string Type,
    StripeCardDetailsDto? Card);

public record StripeChargeOutcomeDto(
    string? NetworkStatus,
    string? Reason,
    string? RiskLevel,
    long? RiskScore,
    string? SellerMessage,
    string? Type);

public record StripeChargeDto(
    string Id,
    long Amount,
    long AmountCaptured,
    long AmountRefunded,
    string? BalanceTransaction,
    StripeBillingDetailsDto? BillingDetails,
    bool Captured,
    DateTime Created,
    string Currency,
    string? Customer,
    string? Description,
    bool Disputed,
    string? FailureCode,
    string? FailureMessage,
    bool Paid,
    string? PaymentIntent,
    string? PaymentMethod,
    StripePaymentMethodDetailsDto? PaymentMethodDetails,
    string? ReceiptEmail,
    string? ReceiptNumber,
    string? ReceiptUrl,
    bool Refunded,
    string? StatementDescriptor,
    string? StatementDescriptorSuffix,
    string Status,
    StripeChargeOutcomeDto? Outcome);

public record StripeChargeListDto(
    IReadOnlyList<StripeChargeDto> Data,
    bool HasMore,
    string? LastId);
