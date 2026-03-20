using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Cart.Commands.RemoveCoupon;

public class RemoveCouponCommandHandler(
    ICartRepository cartRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<RemoveCouponCommand, Result>
{
    public async Task<Result> Handle(RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        CartEntity? cart = null;

        if (request.UserId.HasValue)
            cart = await cartRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
            cart = await cartRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);

        if (cart is null)
            return Result.NotFound("Cart", "current user");

        if (cart.CouponCode is null)
            return Result.BusinessRuleViolation(NoCouponApplied, "No coupon is applied to this cart.");

        cart.CouponCode = null;
        cart.UpdatedAt = DateTime.UtcNow;
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
