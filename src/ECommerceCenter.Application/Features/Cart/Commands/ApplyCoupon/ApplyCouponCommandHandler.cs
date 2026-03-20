using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Cart.Commands.ApplyCoupon;

public class ApplyCouponCommandHandler(
    ICartRepository cartRepository,
    ICouponEvaluator couponEvaluator,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<ApplyCouponCommand, Result>
{
    public async Task<Result> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Resolve current cart items (for line-total evaluation) ─────────
        var header = await cartRepository.GetCartHeaderAsync(request.UserId, request.SessionId, cancellationToken);
        if (header is null)
            return Result.NotFound("Cart", "current user");

        var rows = await cartRepository.GetCartItemsProjectedAsync(header.Value.Id, cancellationToken);

        if (rows.Count == 0)
            return Result.BusinessRuleViolation(EmptyCart, "Cannot apply a coupon to an empty cart.");

        // ── 2. Evaluate coupon ────────────────────────────────────────────────
        var evalItems = rows
            .Select(r => new CartItemForEvaluation(r.VariantId, Math.Round(r.BasePrice * r.Quantity, 2)))
            .ToList();

        var subtotal = evalItems.Sum(e => e.LineTotal);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var eval = await couponEvaluator.EvaluateAsync(
            normalizedCode, evalItems, subtotal, request.UserId, request.SessionId, cancellationToken);

        if (!eval.IsValid)
            return Result.BusinessRuleViolation(eval.FailureCode!.Value, eval.FailureReason!);

        // ── 3. Attach coupon code to cart ────────────────────────────────────
        CartEntity? cart = null;

        if (request.UserId.HasValue)
            cart = await cartRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
            cart = await cartRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);

        if (cart is null)
            return Result.NotFound("Cart", "current user");

        cart.CouponCode = normalizedCode;
        cart.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
