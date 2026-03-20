using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? CategorySlug = null,
    string? Search = null,
    string? Brand = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string SortBy = "relevance",
    bool? InStock = null) : IRequest<Result<PaginatedList<ProductListItemDto>>>;
