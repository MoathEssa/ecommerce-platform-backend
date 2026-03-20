using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand(int? UserId, string? SessionId) : IRequest<Result>;
