using System.Net.Http.Json;
using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping.Models;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping;

/// <summary>
/// Calls the CJ Dropshipping <c>POST /v1/logistic/freightCalculate</c> endpoint to
/// retrieve available shipping options for a set of variant/quantity pairs.
/// </summary>
public sealed class CjDropshippingFreightService(
    IHttpClientFactory httpClientFactory,
    ICjAccessTokenProvider tokenProvider,
    ILogger<CjDropshippingFreightService> logger) : ICjFreightService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<FreightOptionDto>> CalculateFreightAsync(
        string startCountryCode,
        string endCountryCode,
        string? zip,
        IReadOnlyList<CjFreightItemRequest> items,
        CancellationToken ct = default)
    {
        var token  = await tokenProvider.GetCurrentTokenAsync(ct);
        var client = httpClientFactory.CreateClient("CjDropshipping");

        var payload = new CjFreightCalculatePayload(
            startCountryCode,
            endCountryCode,
            zip,
            items.Select(i => new CjFreightItemPayload(i.Vid, i.Quantity)).ToList());

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/logistic/freightCalculate");
        request.Headers.Add("CJ-Access-Token", token);
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var envelope = JsonSerializer.Deserialize<CjApiResponse<List<CjFreightOption>>>(json, JsonOptions);

        if (envelope is null || !envelope.Result || envelope.Data is null)
        {
            logger.LogWarning(
                "CJ freight calculation returned an unexpected response. Code={Code} Message={Message}",
                envelope?.Code, envelope?.Message);
            return [];
        }

        return envelope.Data
            .Select(o => new FreightOptionDto(
                o.LogisticName,
                o.LogisticPrice,
                o.LogisticAging,
                o.TaxesFee,
                o.ClearanceOperationFee,
                o.TotalPostageFee))
            .ToList();
    }
}
