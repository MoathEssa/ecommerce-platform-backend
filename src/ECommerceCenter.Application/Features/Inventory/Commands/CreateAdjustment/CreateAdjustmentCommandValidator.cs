using FluentValidation;

namespace ECommerceCenter.Application.Features.Inventory.Commands.CreateAdjustment;

public class CreateAdjustmentCommandValidator : AbstractValidator<CreateAdjustmentCommand>
{
    public CreateAdjustmentCommandValidator()
    {
        RuleFor(x => x.VariantId).GreaterThan(0);
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("Delta must be non-zero.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
    }
}
