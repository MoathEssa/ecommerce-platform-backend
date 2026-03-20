using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.Images.Commands.ReorderProductImages;

public class ReorderProductImagesCommandValidator : AbstractValidator<ReorderProductImagesCommand>
{
    public ReorderProductImagesCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);

        RuleFor(x => x.ImageIds)
            .NotEmpty()
            .Must(ids => ids.Count == ids.Distinct().Count())
            .WithMessage("Image IDs must be unique.");
    }
}
