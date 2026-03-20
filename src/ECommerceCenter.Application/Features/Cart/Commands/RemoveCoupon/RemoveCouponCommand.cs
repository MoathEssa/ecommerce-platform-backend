using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.RemoveCoupon;

public record RemoveCouponCommand(int? UserId, string? SessionId) : IRequest<Result>;
