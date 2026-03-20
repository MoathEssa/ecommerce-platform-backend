using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Queries.GetCjProducts;

public record GetCjProductsQuery(
    string? KeyWord = null,
    int Page = 1,
    int Size = 20,
    string? CategoryId = null,
    string? CountryCode = null,
    decimal? StartSellPrice = null,
    decimal? EndSellPrice = null,
    int? AddMarkStatus = null,
    int? ProductType = null,
    string? Sort = null,
    int? OrderBy = null) : IRequest<Result<CjProductListResult>>;

public class GetCjProductsQueryHandler(ICjProductService cjProductService)
    : IRequestHandler<GetCjProductsQuery, Result<CjProductListResult>>
{
    public async Task<Result<CjProductListResult>> Handle(
        GetCjProductsQuery request, CancellationToken cancellationToken)
    {
        var searchParams = new CjProductSearchParams(
            KeyWord: request.KeyWord,
            Page: request.Page,
            Size: request.Size,
            CategoryId: request.CategoryId,
            CountryCode: request.CountryCode,
            StartSellPrice: request.StartSellPrice,
            EndSellPrice: request.EndSellPrice,
            AddMarkStatus: request.AddMarkStatus,
            ProductType: request.ProductType,
            Sort: request.Sort,
            OrderBy: request.OrderBy);

        var result = await cjProductService.SearchProductsAsync(searchParams, cancellationToken);
        return Result<CjProductListResult>.Success(result);
    }
}
