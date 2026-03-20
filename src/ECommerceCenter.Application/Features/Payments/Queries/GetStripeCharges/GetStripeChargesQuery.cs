using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Queries.GetStripeCharges;

public record GetStripeChargesQuery(int Limit = 20, string? StartingAfter = null)
    : IRequest<Result<StripeChargeListDto>>;
