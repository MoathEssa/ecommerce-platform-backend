using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Checkout.Queries.CalculateFreight;

public class CalculateFreightQueryHandler(
    IProductVariantRepository variantRepository,
    ICjFreightService freightService)
    : IRequestHandler<CalculateFreightQuery, Result<List<FreightOptionDto>>>
{
    public async Task<Result<List<FreightOptionDto>>> Handle(
        CalculateFreightQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve internal variant IDs → CJ external VIDs (ExternalSkuId)
        var variantIds = request.Items.Select(i => i.VariantId).Distinct().ToList();
        var variantMap = await variantRepository.GetWithProductsAsync(variantIds, cancellationToken);

        var freightItems = new List<CjFreightItemRequest>();

        foreach (var item in request.Items)
        {
            if (!variantMap.TryGetValue(item.VariantId, out var variant))
                return Result<List<FreightOptionDto>>.Failure(
                    new Error("VariantNotFound", $"Variant {item.VariantId} was not found or is inactive."),
                    System.Net.HttpStatusCode.BadRequest);

            // Own-brand variants have no CJ linkage — skip them silently
            if (string.IsNullOrWhiteSpace(variant.ExternalSkuId))
                continue;

            freightItems.Add(new CjFreightItemRequest(variant.ExternalSkuId, item.Quantity));
        }

        // No CJ-linked variants in the cart — return empty list (no external shipping needed)
        if (freightItems.Count == 0)
            return Result<List<FreightOptionDto>>.Success([]);

        var options = await freightService.CalculateFreightAsync(
            startCountryCode: "CN",
            endCountryCode: request.EndCountryCode.ToUpperInvariant(),
            zip: request.Zip,
            items: freightItems,
            ct: cancellationToken);

        return Result<List<FreightOptionDto>>.Success(options);
    }
}
