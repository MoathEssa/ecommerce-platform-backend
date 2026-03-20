using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Queries.GetProductImages;

public class GetProductImagesQueryHandler(
    IProductRepository productRepository,
    IProductImageRepository imageRepository)
    : IRequestHandler<GetProductImagesQuery, Result<List<ProductImageDto>>>
{
    public async Task<Result<List<ProductImageDto>>> Handle(
        GetProductImagesQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<List<ProductImageDto>>.NotFound("Product", request.ProductId);

        var images = await imageRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        var dtos = images
            .OrderBy(i => i.SortOrder)
            .Select(i => new ProductImageDto(i.Id, i.Url, i.VariantId, i.SortOrder))
            .ToList();

        return Result<List<ProductImageDto>>.Success(dtos);
    }
}
