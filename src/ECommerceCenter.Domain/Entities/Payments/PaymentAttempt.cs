using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Payments;

public class PaymentAttempt
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    /// <summary>e.g. Stripe</summary>
    public string Provider { get; set; } = string.Empty;
    public string ProviderIntentId { get; set; } = string.Empty;
    public PaymentAttemptStatus Status { get; set; }
    public decimal Amount { get; set; }
    /// <summary>ISO 4217 e.g. SAR</summary>
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public ICollection<Refund> Refunds { get; set; } = [];
}
