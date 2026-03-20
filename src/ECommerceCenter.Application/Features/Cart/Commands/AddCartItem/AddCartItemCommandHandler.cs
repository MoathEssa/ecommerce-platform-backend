using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Application.Common.Settings;
using ECommerceCenter.Domain.Entities.Cart;
using ECommerceCenter.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Options;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Cart.Commands.AddCartItem;

public class AddCartItemCommandHandler(
    ICartRepository cartRepository,
    IProductVariantRepository variantRepository,
    IInventoryItemRepository inventoryRepository,
    IOptions<StoreSettings> storeSettings,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<AddCartItemCommand, Result>
{
    private const int MaxQuantityPerVariant = 99;

    public async Task<Result> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Validate variant and product ──────────────────────────────────
        var variant = await variantRepository.GetWithProductAsync(request.VariantId, cancellationToken);

        if (variant is null || !variant.IsActive)
            return Result.NotFound("Variant", request.VariantId);

        if (variant.Product.Status != ProductStatus.Active)
            return Result.BusinessRuleViolation(ProductUnavailable, "The product for this variant is not available.");

        // ── 2. Load cart (needed for cumulative qty check) ────────────────────
        CartEntity? cart = null;

        if (request.UserId.HasValue)
            cart = await cartRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
            cart = await cartRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);

        // ── 3. Stock validation ──────────────────────────────────────────────
        var inventory = await inventoryRepository.GetByVariantIdAsync(request.VariantId, cancellationToken);

        // When no inventory record exists (e.g. CJ dropshipping products imported before
        // inventory tracking was added) treat stock as unlimited and allow the purchase.
        if (inventory is not null)
        {
            var available = inventory.OnHand;

            if (available <= 0)
                return Result.BusinessRuleViolation(OutOfStock, "This item is currently out of stock.");

            var existingQty = cart?.Items.FirstOrDefault(i => i.VariantId == request.VariantId)?.Quantity ?? 0;
            if (existingQty + request.Quantity > available)
                return Result.BusinessRuleViolation(
                    InsufficientStock,
                    $"Only {Math.Max(0, available - existingQty)} more unit(s) can be added (available: {available}).");
        }

        // ── 4. Mutate cart inside a transaction ──────────────────────────────
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (cart is null)
            {
                cart = new CartEntity
                {
                    UserId = request.UserId,
                    SessionId = request.UserId.HasValue ? null : request.SessionId,
                    CurrencyCode = storeSettings.Value.CurrencyCode,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await cartRepository.AddAsync(cart, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.VariantId == request.VariantId);
            if (existingItem is not null)
                existingItem.Quantity = Math.Min(existingItem.Quantity + request.Quantity, MaxQuantityPerVariant);
            else
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    VariantId = request.VariantId,
                    Quantity = Math.Min(request.Quantity, MaxQuantityPerVariant),
                    CreatedAt = DateTime.UtcNow
                });

            cart.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return Result.Success();
    }
}
