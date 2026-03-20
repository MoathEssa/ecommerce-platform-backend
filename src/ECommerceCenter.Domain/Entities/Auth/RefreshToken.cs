namespace ECommerceCenter.Domain.Entities.Auth;

/// <summary>
/// Refresh token entity for JWT token management.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
    public string? RevokedReason { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Computed — not stored in DB
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
