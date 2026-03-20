namespace ECommerceCenter.Application.Abstractions.Services.Suppliers;

/// <summary>
/// Provides a valid (non-expired) CJDropshipping access token by reading the
/// stored <c>SupplierCredential</c> and refreshing it when necessary.
/// </summary>
public interface ICjAccessTokenProvider
{
    /// <summary>
    /// Returns a non-expired access token for the CJDropshipping API.
    /// Refreshes the credential automatically if the current token is expired.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no active CJDropshipping credential is found in the database.
    /// </exception>
    Task<string> GetCurrentTokenAsync(CancellationToken ct = default);
}
