namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>Category reference projected inside product-detail read methods.</summary>
public record CatalogCategoryRefRow(int CategoryId, string Name, string Slug);
