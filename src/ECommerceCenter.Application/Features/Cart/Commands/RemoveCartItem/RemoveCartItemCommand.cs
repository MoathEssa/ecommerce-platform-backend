using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(
    int? UserId,
    string? SessionId,
    int ItemId) : IRequest<Result>;
