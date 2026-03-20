using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.AddVariant;

public class AddVariantCommandValidator : AbstractValidator<AddVariantCommand>
{
    public AddVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);

        RuleFor(x => x.Options)
            .NotNull();

        RuleFor(x => x.BasePrice)
            .GreaterThan(0);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Currency code must be a 3-letter ISO 4217 code (e.g. SAR, USD).");

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0);
    }
}
