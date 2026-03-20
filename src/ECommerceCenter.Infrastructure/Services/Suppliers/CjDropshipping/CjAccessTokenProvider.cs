using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Domain.Entities.Suppliers;
using ECommerceCenter.Domain.Enums;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping;

/// <summary>
/// Retrieves the current CJDropshipping access token, refreshing it via
/// <see cref="ISupplierAuthService"/> when the stored token is expired.
/// </summary>
public sealed class CjAccessTokenProvider(
    AppDbContext db,
    SupplierAuthServiceFactory supplierAuthServiceFactory,
    ILogger<CjAccessTokenProvider> logger) : ICjAccessTokenProvider
{
    public async Task<string> GetCurrentTokenAsync(CancellationToken ct = default)
    {
        var credential = await db.SupplierCredentials
            .FirstOrDefaultAsync(c => c.SupplierType == SupplierType.CjDropshipping
                                   && c.IsActive, ct)
            ?? throw new InvalidOperationException(
                "No active CJDropshipping credential found. " +
                "Please save your CJ API key via the supplier settings.");

        // Token still valid — return immediately
        if (!string.IsNullOrEmpty(credential.AccessToken) && !credential.IsAccessTokenExpired)
            return credential.AccessToken;

        var authService = supplierAuthServiceFactory.GetService(SupplierType.CjDropshipping);

        // Refresh token still valid — use it
        if (!credential.IsRefreshTokenExpired && !string.IsNullOrEmpty(credential.RefreshToken))
        {
            logger.LogInformation("Refreshing CJDropshipping access token via refresh token.");
            var refreshed = await authService.RefreshAccessTokenAsync(credential.RefreshToken, ct);
            ApplyTokenResult(credential, refreshed);
        }
        else
        {
            // Fall back to full re-authentication with the stored API key
            logger.LogWarning(
                "CJDropshipping refresh token expired or missing; re-authenticating with API key.");
            var freshResult = await authService.GetAccessTokenAsync(credential.ApiKey, ct);
            ApplyTokenResult(credential, freshResult);
        }

        await db.SaveChangesAsync(ct);
        return credential.AccessToken!;
    }

    private static void ApplyTokenResult(SupplierCredential credential, SupplierTokenResult result)
    {
        credential.AccessToken            = result.AccessToken;
        credential.AccessTokenExpiryDate  = result.AccessTokenExpiryDate;
        credential.RefreshToken           = result.RefreshToken;
        credential.RefreshTokenExpiryDate = result.RefreshTokenExpiryDate;
        credential.LastRefreshedAt        = DateTime.UtcNow;
        if (result.OpenId.HasValue)
            credential.OpenId = result.OpenId;
    }
}

