using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Reliability;

public class AuditLog
{
    public int Id { get; set; }
    public int? ActorId { get; set; }
    public ActorType ActorType { get; set; }
    /// <summary>e.g. Product.Update, Order.Ship, Inventory.Adjust</summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>e.g. Product, Order</summary>
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Optional correlation / request trace Id.</summary>
    public string? CorrelationId { get; set; }
}
