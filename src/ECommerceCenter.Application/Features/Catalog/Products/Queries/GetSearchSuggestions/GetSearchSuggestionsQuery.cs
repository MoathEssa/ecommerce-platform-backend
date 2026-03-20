using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetSearchSuggestions;

public record GetSearchSuggestionsQuery(string Q, int Limit = 8)
    : IRequest<Result<List<SearchSuggestionDto>>>;
