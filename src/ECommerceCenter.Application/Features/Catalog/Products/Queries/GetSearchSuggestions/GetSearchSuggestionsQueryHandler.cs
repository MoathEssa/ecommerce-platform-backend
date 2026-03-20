using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetSearchSuggestions;

public class GetSearchSuggestionsQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<GetSearchSuggestionsQuery, Result<List<SearchSuggestionDto>>>
{
    public async Task<Result<List<SearchSuggestionDto>>> Handle(
        GetSearchSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        var term = request.Q.Trim();

        var productRows = await productRepository.SearchByTitleAsync(term, request.Limit, cancellationToken);

        var remaining = request.Limit - productRows.Count;
        var categoryRows = remaining > 0
            ? await categoryRepository.SearchByNameAsync(term, remaining, cancellationToken)
            : [];

        var suggestions = productRows.Select(r => new SearchSuggestionDto("product", r.Title, r.Slug))
            .Concat(categoryRows.Select(r => new SearchSuggestionDto("category", r.Title, r.Slug)))
            .ToList();

        return Result<List<SearchSuggestionDto>>.Success(suggestions);
    }
}
