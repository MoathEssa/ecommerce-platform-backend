using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Application.Abstractions.Services.Suppliers;


public record SupplierTokenResult(
    long? OpenId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiryDate,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiryDate,
    DateTimeOffset CreatedDate);

/// <summary>
/// Contract every supplier authentication adapter must satisfy.
/// Implement this interface once per supplier in Infrastructure; the factory resolves the correct one at runtime.
/// </summary>
public interface ISupplierAuthService
{
    /// <summary>Identifies which supplier this implementation handles. Used by the factory for resolution.</summary>
    SupplierType SupplierType { get; }

    Task<SupplierTokenResult> GetAccessTokenAsync(
        string apiKey,
        CancellationToken cancellationToken = default);

    Task<SupplierTokenResult> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
