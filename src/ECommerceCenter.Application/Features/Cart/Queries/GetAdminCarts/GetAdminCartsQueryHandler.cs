using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Common.Pagination;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Queries.GetAdminCarts;

public class GetAdminCartsQueryHandler(ICartRepository cartRepository)
    : IRequestHandler<GetAdminCartsQuery, Result<PaginatedList<AdminCartListItemDto>>>
{
    public async Task<Result<PaginatedList<AdminCartListItemDto>>> Handle(
        GetAdminCartsQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await cartRepository.GetAdminCartsAsync(
            page, pageSize, request.Search, request.Status, cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Result<PaginatedList<AdminCartListItemDto>>.Success(
            new PaginatedList<AdminCartListItemDto>(items, page, pageSize, totalCount, totalPages));
    }
}
