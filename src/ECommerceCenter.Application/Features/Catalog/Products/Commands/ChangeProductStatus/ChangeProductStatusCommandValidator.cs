using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.ChangeProductStatus;

public class ChangeProductStatusCommandValidator : AbstractValidator<ChangeProductStatusCommand>
{
    public ChangeProductStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Status)
            .Must(s => s == 1 || s == 2 || s == 3)
            .WithMessage("Status must be 1 (Draft), 2 (Active), or 3 (Archived).");
    }
}
