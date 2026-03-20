using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Queries.GetCart;


public record GetCartQuery(int? UserId, string? SessionId) : IRequest<Result<CartDto>>;
