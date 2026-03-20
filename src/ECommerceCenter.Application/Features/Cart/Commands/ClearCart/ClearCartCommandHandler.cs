using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Cart.Commands.ClearCart;

public class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<ClearCartCommand, Result>
{
    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        CartEntity? cart = null;

        if (request.UserId.HasValue)
            cart = await cartRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
            cart = await cartRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);

        if (cart is null)
            return Result.NotFound("Cart", "current user");

        // Bulk DELETE — single SQL statement, no per-row tracking
        await cartRepository.ClearItemsAsync(cart.Id, cancellationToken);

        cart.CouponCode = null;
        cart.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
