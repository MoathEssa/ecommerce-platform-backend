using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Commands.DeactivateCoupon;

public record DeactivateCouponCommand(int Id, int ActorId) : IRequest<Result>;
