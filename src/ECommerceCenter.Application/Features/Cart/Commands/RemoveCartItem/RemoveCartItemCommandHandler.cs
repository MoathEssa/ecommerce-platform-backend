using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Cart;
using MediatR;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Cart.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler(
    ICartRepository cartRepository,
    IGenericRepository<CartItem> cartItemRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<RemoveCartItemCommand, Result>
{
    public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        CartEntity? cart = null;

        if (request.UserId.HasValue)
            cart = await cartRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
            cart = await cartRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);

        if (cart is null)
            return Result.NotFound("Cart", "current user");

        var item = cart.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null)
            return Result.NotFound("CartItem", request.ItemId);

        cartItemRepository.Delete(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
