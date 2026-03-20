using System.Net.Http.Json;
using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Domain.Enums;
using ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping.Models;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping;

/// <summary>
/// Adapts the CJDropshipping REST API v2.0 auth endpoints to <see cref="ISupplierAuthService"/>.
/// Uses a named <see cref="HttpClient"/> registered as "CjDropshipping".
/// </summary>
public sealed class CjDropshippingAuthService(
    IHttpClientFactory httpClientFactory,
    ILogger<CjDropshippingAuthService> logger) : ISupplierAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SupplierType SupplierType => SupplierType.CjDropshipping;

    // ── Public interface ──────────────────────────────────────────────────

    public async Task<SupplierTokenResult> GetAccessTokenAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var body = new { apiKey };
        var data = await PostAsync<CjTokenData>(
            "v1/authentication/getAccessToken",
            body,
            accessToken: null,
            cancellationToken);

        logger.LogInformation(
            "CJDropshipping access token obtained. OpenId={OpenId} ExpiresAt={ExpiresAt}",
            data.OpenId, data.AccessTokenExpiryDate);

        return MapToResult(data);
    }

    public async Task<SupplierTokenResult> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var body = new { refreshToken };
        var data = await PostAsync<CjTokenData>(
            "v1/authentication/refreshAccessToken",
            body,
            accessToken: null,
            cancellationToken);

        logger.LogInformation(
            "CJDropshipping access token refreshed. ExpiresAt={ExpiresAt}",
            data.AccessTokenExpiryDate);

        return MapToResult(data);
    }

    public async Task LogoutAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        // Logout sends the token as a header, not a body — handled via PostAsync overload.
        var client = httpClientFactory.CreateClient("CjDropshipping");
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/authentication/logout");
        request.Headers.Add("CJ-Access-Token", accessToken);

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var envelope = JsonSerializer.Deserialize<CjApiResponse<bool>>(json, JsonOptions);

        if (envelope is null || !envelope.Result)
        {
            logger.LogWarning(
                "CJDropshipping logout failed. Code={Code} Message={Message} RequestId={RequestId}",
                envelope?.Code, envelope?.Message, envelope?.RequestId);
            throw new InvalidOperationException(
                $"CJDropshipping logout failed: {envelope?.Message ?? "unknown error"} (requestId={envelope?.RequestId})");
        }

        logger.LogInformation("CJDropshipping logout successful.");
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private async Task<T> PostAsync<T>(
        string relativeUrl,
        object body,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("CjDropshipping");

        using var request = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
        {
            Content = JsonContent.Create(body)
        };

        if (accessToken is not null)
            request.Headers.Add("CJ-Access-Token", accessToken);

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var envelope = JsonSerializer.Deserialize<CjApiResponse<T>>(json, JsonOptions);

        if (envelope is null || !envelope.Result || envelope.Data is null)
        {
            logger.LogError(
                "CJDropshipping API error at {Url}. Code={Code} Message={Message} RequestId={RequestId}",
                relativeUrl, envelope?.Code, envelope?.Message, envelope?.RequestId);
            throw new InvalidOperationException(
                $"CJDropshipping error [{envelope?.Code}]: {envelope?.Message ?? "unknown"} (requestId={envelope?.RequestId})");
        }

        return envelope.Data;
    }

    private static SupplierTokenResult MapToResult(CjTokenData d) =>
        new(d.OpenId, d.AccessToken, d.AccessTokenExpiryDate,
            d.RefreshToken, d.RefreshTokenExpiryDate, d.CreateDate);
}
