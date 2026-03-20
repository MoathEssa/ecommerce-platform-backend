using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Queries.GetCjProductVariants;

public record GetCjProductVariantsQuery(
    string Pid,
    string? CountryCode = null) : IRequest<Result<List<CjProductVariantDto>>>;

public class GetCjProductVariantsQueryHandler(ICjProductService cjProductService)
    : IRequestHandler<GetCjProductVariantsQuery, Result<List<CjProductVariantDto>>>
{
    public async Task<Result<List<CjProductVariantDto>>> Handle(
        GetCjProductVariantsQuery request, CancellationToken cancellationToken)
    {
        var variants = await cjProductService.GetProductVariantsAsync(
            request.Pid,
            request.CountryCode,
            cancellationToken);

        return Result<List<CjProductVariantDto>>.Success(variants);
    }
}
