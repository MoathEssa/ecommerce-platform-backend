using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetAdminProducts;

public class GetAdminProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetAdminProductsQuery, Result<PaginatedList<AdminProductListItemDto>>>
{
    public async Task<Result<PaginatedList<AdminProductListItemDto>>> Handle(
        GetAdminProductsQuery request, CancellationToken cancellationToken)
    {
        var filter = new AdminProductListFilter(
            request.Page, request.PageSize, request.Status, request.Search, request.SortBy);

        var (rows, totalCount) = await productRepository.GetAdminProductListingPagedAsync(filter, cancellationToken);

        var items = rows.Select(row => new AdminProductListItemDto(
            row.Id, row.Title, row.Slug, row.Brand, row.Status,
            row.ActiveVariantCount, row.CoverImageUrl, row.CreatedAt)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
        return Result<PaginatedList<AdminProductListItemDto>>.Success(
            new PaginatedList<AdminProductListItemDto>(items, filter.Page, filter.PageSize, totalCount, totalPages));
    }
}
