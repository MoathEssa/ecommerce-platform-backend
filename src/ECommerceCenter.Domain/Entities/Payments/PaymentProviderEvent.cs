namespace ECommerceCenter.Domain.Entities.Payments;

public class PaymentProviderEvent
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    /// <summary>Provider-unique event Id (e.g. Stripe evt_*). Used for webhook dedupe.</summary>
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    /// <summary>Provider payment intent Id this event relates to.</summary>
    public string? RelatedIntentId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
