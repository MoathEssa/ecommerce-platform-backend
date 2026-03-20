using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetAdminProducts;

public class GetAdminProductsQueryValidator : AbstractValidator<GetAdminProductsQuery>
{
    private static readonly string[] ValidStatuses  = ["Draft", "Active", "Archived"];
    private static readonly string[] ValidSortBys   = ["newest", "title-asc", "title-desc"];

    public GetAdminProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.SortBy)
            .Must(s => ValidSortBys.Contains(s))
            .WithMessage($"SortBy must be one of: {string.Join(", ", ValidSortBys)}.");
    }
}
