using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetAdminProducts;

public record GetAdminProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Search = null,
    string SortBy = "newest") : IRequest<Result<PaginatedList<AdminProductListItemDto>>>;
