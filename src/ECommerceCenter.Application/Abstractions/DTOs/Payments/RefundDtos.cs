namespace ECommerceCenter.Application.Abstractions.DTOs.Payments;

public record RefundDto(
    int Id,
    int OrderId,
    decimal Amount,
    string CurrencyCode,
    string Status,
    string? Reason,
    string? ProviderRefundId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
