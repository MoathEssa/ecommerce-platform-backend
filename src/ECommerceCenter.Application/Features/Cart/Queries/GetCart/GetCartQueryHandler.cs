using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Queries.GetCart;

public class GetCartQueryHandler(
    ICartRepository cartRepository,
    ICouponEvaluator couponEvaluator)
    : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var header = await cartRepository.GetCartHeaderAsync(request.UserId, request.SessionId, cancellationToken);
        if (header is null)
            return Result<CartDto>.Success(EmptyCart());

        var (cartId, couponCode, currencyCode) = header.Value;

        var rows = await cartRepository.GetCartItemsProjectedAsync(cartId, cancellationToken);

        var itemDtos = rows.Select(row =>
        {
            string stockStatus;
            var warnings = new List<string>();

            if (!row.VariantIsActive || row.ProductStatus != ProductStatus.Active)
            {
                stockStatus = "unavailable";
                warnings.Add("This item is no longer available.");
            }
            else
            {
                stockStatus = StockThresholds.Map(row.AvailableStock);
                if (stockStatus == StockStatus.OutOfStock)
                    warnings.Add("This item is currently out of stock.");
            }

            var options = string.IsNullOrWhiteSpace(row.OptionsJson)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(row.OptionsJson)
                  ?? new Dictionary<string, string>();

            var lineTotal = Math.Round(row.BasePrice * row.Quantity, 2);

            return new CartItemDto(
                row.Id, row.VariantId, row.ProductId, row.ProductTitle, row.ProductSlug,
                row.Sku, options, row.ImageUrl, row.BasePrice, row.Quantity,
                lineTotal, stockStatus, warnings);
        }).ToList();

        var subtotal = itemDtos.Sum(i => i.LineTotal);
        CartCouponDto? couponDto = null;
        var discountTotal = 0m;

        if (!string.IsNullOrWhiteSpace(couponCode) && itemDtos.Count > 0)
        {
            var evalItems = itemDtos
                .Select(i => new CartItemForEvaluation(i.VariantId, i.LineTotal))
                .ToList();

            var eval = await couponEvaluator.EvaluateAsync(
                couponCode, evalItems, subtotal, request.UserId, request.SessionId, cancellationToken);

            if (eval.IsValid)
            {
                couponDto = eval.CouponDto;
                discountTotal = eval.DiscountAmount;
            }
        }

        var total = Math.Round(subtotal - discountTotal, 2);
        var itemCount = itemDtos.Sum(i => i.Quantity);

        return Result<CartDto>.Success(
            new CartDto(cartId, currencyCode, itemDtos, couponDto, subtotal, discountTotal, total, itemCount));
    }

    private static CartDto EmptyCart() => new(0, "SAR", [], null, 0m, 0m, 0m, 0);
}
