using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Slug)
            .MaximumLength(320)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens only.")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Brand)
            .MaximumLength(150)
            .When(x => x.Brand is not null);
    }
}
