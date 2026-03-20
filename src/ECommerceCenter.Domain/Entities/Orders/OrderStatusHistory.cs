using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Orders;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    /// <summary>
    /// The status the order transitioned FROM. Null for the initial "order created" entry
    /// because there is no prior state.
    /// </summary>
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    /// <summary>Admin / User / System actor Id.</summary>
    public int? ChangedBy { get; set; }
    public ActorType ChangedByType { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
}
