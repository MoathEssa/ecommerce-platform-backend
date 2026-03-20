using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProductReviews;

public class GetProductReviewsQueryHandler(
    IProductRepository productRepository,
    ICjProductService cjProductService)
    : IRequestHandler<GetProductReviewsQuery, Result<ReviewListDto>>
{
    public async Task<Result<ReviewListDto>> Handle(
        GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var row = await productRepository.GetProductDetailBySlugAsync(request.Slug, cancellationToken);

        if (row is null)
            return Result<ReviewListDto>.NotFound("Product", request.Slug);

        // Non-CJ products have no CJ reviews
        if (string.IsNullOrWhiteSpace(row.ExternalProductId) ||
            row.Supplier != (int)SupplierType.CjDropshipping)
        {
            return Result<ReviewListDto>.Success(
                new ReviewListDto(request.Page, request.PageSize, 0, 0.0, []));
        }

        var cjResult = await cjProductService.GetProductReviewsAsync(
            row.ExternalProductId, request.Page, request.PageSize, cancellationToken);

        var avgScore = cjResult.Items.Count > 0
            ? Math.Round(cjResult.Items.Average(i => i.Score), 1)
            : 0.0;

        var dto = new ReviewListDto(
            PageNum:      request.Page,
            PageSize:     request.PageSize,
            Total:        cjResult.Total,
            AverageScore: avgScore,
            Items: cjResult.Items
                .Select(i => new ReviewDto(
                    i.CommentId, i.CommentUser, i.Score,
                    i.Comment, i.CommentDate, i.CountryCode,
                    i.FlagIconUrl, i.CommentUrls))
                .ToList());

        return Result<ReviewListDto>.Success(dto);
    }
}
