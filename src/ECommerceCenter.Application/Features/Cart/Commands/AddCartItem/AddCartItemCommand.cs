using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.AddCartItem;

public record AddCartItemCommand(
    int? UserId,
    string? SessionId,
    int VariantId,
    int Quantity) : IRequest<Result>;
