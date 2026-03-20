namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>
/// Minimal projection for search-suggestion queries.
/// The handler adds the "type" label and builds <c>SearchSuggestionDto</c>.
/// </summary>
public record CatalogSearchRow(string Title, string Slug);
