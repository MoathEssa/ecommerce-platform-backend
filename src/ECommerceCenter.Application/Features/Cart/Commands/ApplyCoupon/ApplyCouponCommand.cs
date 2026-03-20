using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Cart.Commands.ApplyCoupon;

public record ApplyCouponCommand(
    int? UserId,
    string? SessionId,
    string Code) : IRequest<Result>;
