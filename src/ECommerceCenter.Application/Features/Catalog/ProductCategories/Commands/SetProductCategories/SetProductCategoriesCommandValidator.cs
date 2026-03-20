using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.ProductCategories.Commands.SetProductCategories;

public class SetProductCategoriesCommandValidator : AbstractValidator<SetProductCategoriesCommand>
{
    public SetProductCategoriesCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);

        When(x => x.CategoryId.HasValue, () =>
            RuleFor(x => x.CategoryId!.Value).GreaterThan(0));
    }
}
