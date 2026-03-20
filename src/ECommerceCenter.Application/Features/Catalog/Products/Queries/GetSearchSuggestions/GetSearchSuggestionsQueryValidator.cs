using FluentValidation;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetSearchSuggestions;

public class GetSearchSuggestionsQueryValidator : AbstractValidator<GetSearchSuggestionsQuery>
{
    public GetSearchSuggestionsQueryValidator()
    {
        RuleFor(x => x.Q)
            .NotEmpty()
            .MinimumLength(2)
            .WithMessage("Search query must be at least 2 characters.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 20)
            .WithMessage("Limit must be between 1 and 20.");
    }
}
