using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProducts;

public class GetProductsQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<GetProductsQuery, Result<PaginatedList<ProductListItemDto>>>
{
    public async Task<Result<PaginatedList<ProductListItemDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<int>? categoryIds = null;
        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var all = await categoryRepository.GetAllActiveAsync(cancellationToken);
            var root = all.FirstOrDefault(c => c.Slug == request.CategorySlug);

            if (root is null)
                return Result<PaginatedList<ProductListItemDto>>.Success(
                    new PaginatedList<ProductListItemDto>([], request.Page, request.PageSize, 0, 0));

            categoryIds = GetDescendantCategoryIds(all, root.Id);
        }

        var filter = new ProductListFilter(
            request.Page, request.PageSize, request.CategorySlug,
            request.Search, request.Brand, request.MinPrice,
            request.MaxPrice, request.SortBy, request.InStock);

        var (rows, totalCount) = await productRepository.GetProductListingPagedAsync(filter, categoryIds, cancellationToken);

        var items = rows.Select(row => new ProductListItemDto(
            row.Id, row.Title, row.Slug, row.Brand, row.CoverImageUrl,
            row.MinPrice, row.MaxPrice, row.CurrencyCode, row.HasStock,
            row.PrimaryCategoryId.HasValue
                ? new ProductListCategoryDto(row.PrimaryCategoryName!, row.PrimaryCategorySlug!)
                : null)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
        return Result<PaginatedList<ProductListItemDto>>.Success(
            new PaginatedList<ProductListItemDto>(items, filter.Page, filter.PageSize, totalCount, totalPages));
    }

    private static IReadOnlyCollection<int> GetDescendantCategoryIds(List<Category> all, int rootId)
    {
        var result = new HashSet<int> { rootId };
        var childLookup = all.ToLookup(c => c.ParentId, c => c.Id);
        var stack = new Stack<int>();
        stack.Push(rootId);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            foreach (var childId in childLookup[current])
            {
                result.Add(childId);
                stack.Push(childId);
            }
        }

        return result;
    }
}
