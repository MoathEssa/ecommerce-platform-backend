namespace ECommerceCenter.Domain.Entities.Reliability;

public class IdempotencyKey
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    /// <summary>e.g. POST:/api/v1/checkout</summary>
    public string Route { get; set; } = string.Empty;
    /// <summary>SHA-256 hex of the request body for conflict detection.</summary>
    public string RequestHash { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}
