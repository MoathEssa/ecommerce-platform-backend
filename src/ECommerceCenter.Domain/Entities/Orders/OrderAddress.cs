using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Orders;

public class OrderAddress
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public AddressType Type { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    /// <summary>ISO 3166-1 alpha-2 country code.</summary>
    public string Country { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
}
