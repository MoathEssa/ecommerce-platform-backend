using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Queries.GetCouponDetail;

public record GetCouponDetailQuery(int Id) : IRequest<Result<CouponDetailDto>>;
