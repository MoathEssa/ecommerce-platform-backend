using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.UpdateVariant;

public class UpdateVariantCommandValidator : AbstractValidator<UpdateVariantCommand>
{
    public UpdateVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.VariantId).GreaterThan(0);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Options).NotNull();

        RuleFor(x => x.BasePrice).GreaterThan(0);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Currency code must be a 3-letter ISO 4217 code.");
    }
}
