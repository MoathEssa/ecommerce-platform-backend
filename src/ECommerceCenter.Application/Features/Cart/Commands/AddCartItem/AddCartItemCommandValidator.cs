using FluentValidation;

namespace ECommerceCenter.Application.Features.Cart.Commands.AddCartItem;

public class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.VariantId).GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(99);
    }
}
