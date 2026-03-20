using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(
    int? UserId,
    string? SessionId,
    int ItemId,
    int Quantity) : IRequest<Result>;
