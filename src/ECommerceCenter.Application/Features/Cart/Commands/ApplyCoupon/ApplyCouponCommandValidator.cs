using FluentValidation;

namespace ECommerceCenter.Application.Features.Cart.Commands.ApplyCoupon;

public class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(64);
    }
}
