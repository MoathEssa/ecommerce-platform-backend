using FluentValidation;

namespace ECommerceCenter.Application.Features.Checkout.Commands.PlaceOrder;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.")
            .Must(items => items.Count <= 50).WithMessage("A maximum of 50 items is allowed.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.VariantId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 99);
        });

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // Guest must supply email
        When(x => !x.UserId.HasValue, () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required for guest checkout.");
        });

        RuleFor(x => x.ShippingAddress).NotNull().SetValidator(new AddressDtoValidator());

        When(x => x.BillingAddress is not null, () =>
        {
            RuleFor(x => x.BillingAddress!).SetValidator(new AddressDtoValidator());
        });

        RuleFor(x => x.CouponCode)
            .MaximumLength(100)
            .When(x => x.CouponCode is not null);
    }
}

public class AddressDtoValidator : AbstractValidator<CheckoutAddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone is not null);
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Line2).MaximumLength(200).When(x => x.Line2 is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Region).MaximumLength(120).When(x => x.Region is not null);
        RuleFor(x => x.PostalCode).MaximumLength(30).When(x => x.PostalCode is not null);
        RuleFor(x => x.Country).NotEmpty().Length(2).WithMessage("Country must be an ISO 3166-1 alpha-2 code.");
    }
}
