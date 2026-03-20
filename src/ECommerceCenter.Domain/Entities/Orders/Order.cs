using ECommerceCenter.Domain.Entities.Coupons;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Entities.Shipping;
using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Orders;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    /// <summary>Nullable — guest checkouts are allowed.</summary>
    public int? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    /// <summary>ISO 4217 e.g. SAR</summary>
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal Total { get; set; }
    public string? CouponCode { get; set; }
    public DateTime? CanceledAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderAddress> Addresses { get; set; } = [];
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<PaymentAttempt> PaymentAttempts { get; set; } = [];
    public ICollection<Refund> Refunds { get; set; } = [];
    public ICollection<Shipment> Shipments { get; set; } = [];
    public CouponUsage? CouponUsage { get; set; }
}
