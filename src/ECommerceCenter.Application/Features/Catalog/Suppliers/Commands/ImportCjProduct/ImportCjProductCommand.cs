using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.ImportCjProduct;

/// <summary>
/// Imports a CJDropshipping product into the local store catalogue.
/// The frontend passes all the product details it already has from the CJ product list,
/// so no additional CJ API call is needed.
/// </summary>
public record ImportCjProductCommand(
    string CjProductId,
    string NameEn,
    string? Sku,
    string? ImageUrl,
    string? SellPrice,
    string? CjPrice,
    string? CjCategoryId,
    string? OneCategoryName,
    string? TwoCategoryName,
    string? ThreeCategoryName,
    bool MakeActive = false) : IRequest<Result<int>>;
