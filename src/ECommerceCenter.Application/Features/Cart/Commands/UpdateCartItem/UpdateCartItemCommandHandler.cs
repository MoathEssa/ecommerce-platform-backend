using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler(
    ICartRepository cartRepository,
    IInventoryItemRepository inventoryRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCartItemCommand, Result>
{
    public async Task<Result> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
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

        // ── Stock validation ─────────────────────────────────────────────────
        var inventory = await inventoryRepository.GetByVariantIdAsync(item.VariantId, cancellationToken);
        var available = inventory?.OnHand ?? 0;

        if (available <= 0)
            return Result.BusinessRuleViolation(OutOfStock, "This item is currently out of stock.");

        if (request.Quantity > available)
            return Result.BusinessRuleViolation(
                InsufficientStock,
                $"Only {available} unit(s) are available for this item.");

        item.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
