using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Domain.Entities.Suppliers;


public class SupplierCredential
{
    public int Id { get; set; }

    public SupplierType SupplierType { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public long? OpenId { get; set; }

    public string? AccessToken { get; set; }

    public DateTimeOffset? AccessTokenExpiryDate { get; set; }

    public string? RefreshToken { get; set; }

    public DateTimeOffset? RefreshTokenExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastRefreshedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsAccessTokenExpired =>
        AccessTokenExpiryDate.HasValue && DateTimeOffset.UtcNow >= AccessTokenExpiryDate;

    public bool IsRefreshTokenExpired =>
        RefreshTokenExpiryDate.HasValue && DateTimeOffset.UtcNow >= RefreshTokenExpiryDate;
}
