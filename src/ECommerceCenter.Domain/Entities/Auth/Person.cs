namespace ECommerceCenter.Domain.Entities.Auth;

/// <summary>
/// Profile / personal information for a registered user.
/// 1-to-1 with ApplicationUser — UserId is both PK and FK.
/// </summary>
public class Person
{
    /// <summary>PK = FK to AspNetUsers.Id (no separate identity column).</summary>
    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Profile-level phone (may differ from Identity's PhoneNumber).</summary>
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    /// <summary>URL to profile avatar image.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>e.g. Male / Female / NonBinary / PreferNotToSay</summary>
    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────
    public ApplicationUser User { get; set; } = null!;

    // ── Computed ──────────────────────────────────────────────────────────
    public string FullName => $"{FirstName} {LastName}".Trim();
}
