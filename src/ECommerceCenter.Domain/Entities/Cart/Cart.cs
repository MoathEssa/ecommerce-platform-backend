namespace ECommerceCenter.Domain.Entities.Cart;

public class Cart
{
    public int Id { get; set; }
    /// <summary>Null for guest carts — identified by SessionId instead.</summary>
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    /// <summary>ISO 4217 e.g. SAR</summary>
    public string CurrencyCode { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<CartItem> Items { get; set; } = [];
}
