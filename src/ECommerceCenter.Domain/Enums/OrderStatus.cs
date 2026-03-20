namespace ECommerceCenter.Domain.Enums;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Canceled,
    PartiallyRefunded,
    Refunded
}
