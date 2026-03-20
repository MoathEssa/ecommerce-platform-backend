namespace ECommerceCenter.Domain.Entities.Reliability;

public class OutboxMessage
{
    public int Id { get; set; }
    /// <summary>e.g. OrderPlaced, PaymentSucceeded</summary>
    public string Type { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; } = 0;
    public string? LastError { get; set; }
}
