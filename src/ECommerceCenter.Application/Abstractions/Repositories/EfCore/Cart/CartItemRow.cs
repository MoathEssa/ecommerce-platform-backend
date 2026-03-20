using ECommerceCenter.Domain.Enums;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;

/// <summary>
/// Raw data carrier returned by <see cref="ICartRepository.GetCartItemsProjectedAsync"/>.
/// Contains every field needed to build a <c>CartItemDto</c> in the handler.
/// Never exposed directly over the API.
/// </summary>
public record CartItemRow(
    int Id,
    int VariantId,
    int Quantity,
    bool VariantIsActive,
    string Sku,
    string? OptionsJson,
    decimal BasePrice,
    int ProductId,
    string ProductTitle,
    string ProductSlug,
    ProductStatus ProductStatus,
    int AvailableStock,
    string? ImageUrl);
