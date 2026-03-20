using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Queries.GetAdminCarts;

public record GetAdminCartsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null) : IRequest<Result<PaginatedList<AdminCartListItemDto>>>;
