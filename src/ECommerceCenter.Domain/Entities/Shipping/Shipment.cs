using ECommerceCenter.Domain.Entities.Orders;

namespace ECommerceCenter.Domain.Entities.Shipping;

public class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
}
