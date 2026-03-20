using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Payments;

public class Refund
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? PaymentAttemptId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ProviderRefundId { get; set; }
    public decimal Amount { get; set; }
    /// <summary>ISO 4217 e.g. SAR</summary>
    public string CurrencyCode { get; set; } = string.Empty;
    public RefundStatus Status { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public PaymentAttempt? PaymentAttempt { get; set; }
}
