namespace ECommerceCenter.Application.Abstractions.Services.Suppliers;

/// <summary>A single freight item to send to the CJ calculation API.</summary>
public record CjFreightItemRequest(string Vid, int Quantity);

/// <summary>One shipping option returned by the CJ freight API.</summary>
public record FreightOptionDto(
    string LogisticName,
    decimal LogisticPrice,
    string LogisticAging,
    decimal? TaxesFee,
    decimal? ClearanceOperationFee,
    decimal? TotalPostageFee);

/// <summary>
/// Calls the CJ Dropshipping freight-calculation endpoint and returns the available
/// shipping options for a given set of items and destination address.
/// </summary>
public interface ICjFreightService
{
    /// <summary>
    /// Calculates shipping options for a set of CJ variants shipped from
    /// <paramref name="startCountryCode"/> to <paramref name="endCountryCode"/>.
    /// Returns an empty list when CJ returns no options or the API call fails gracefully.
    /// </summary>
    Task<List<FreightOptionDto>> CalculateFreightAsync(
        string startCountryCode,
        string endCountryCode,
        string? zip,
        IReadOnlyList<CjFreightItemRequest> items,
        CancellationToken ct = default);
}
