using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Queries.GetVariantDetail;

public class GetVariantDetailQueryHandler(IProductVariantRepository variantRepository)
    : IRequestHandler<GetVariantDetailQuery, Result<VariantDetailDto>>
{
    public async Task<Result<VariantDetailDto>> Handle(
        GetVariantDetailQuery request,
        CancellationToken cancellationToken)
    {
        var row = await variantRepository.GetVariantDetailAsync(
            request.ProductSlug, request.VariantId, cancellationToken);

        if (row is null)
            return Result<VariantDetailDto>.NotFound("Variant", request.VariantId);

        var dto = new VariantDetailDto(
            row.Id, row.Sku, OptionsJsonHelper.Parse(row.OptionsJson),
            row.BasePrice, row.CurrencyCode,
            StockThresholds.Map(row.OnHand),
            row.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.VariantId, i.SortOrder)).ToList());

        return Result<VariantDetailDto>.Success(dto);
    }
}
