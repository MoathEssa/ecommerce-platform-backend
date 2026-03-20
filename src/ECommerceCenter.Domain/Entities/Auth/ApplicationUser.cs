using Microsoft.AspNetCore.Identity;

namespace ECommerceCenter.Domain.Entities.Auth;

/// <summary>
/// Custom application user extending ASP.NET Identity.
/// Each user is associated with a unique account.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public Person? Person { get; set; }
}
