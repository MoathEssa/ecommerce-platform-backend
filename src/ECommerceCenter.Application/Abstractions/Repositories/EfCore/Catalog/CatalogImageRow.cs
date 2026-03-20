namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

/// <summary>Minimal image projection shared across product-detail and variant-detail read methods.</summary>
public record CatalogImageRow(int Id, string Url, int? VariantId, int SortOrder);
