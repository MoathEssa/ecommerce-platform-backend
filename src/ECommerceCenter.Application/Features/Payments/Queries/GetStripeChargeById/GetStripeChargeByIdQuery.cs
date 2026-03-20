using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Queries.GetStripeChargeById;

public record GetStripeChargeByIdQuery(string ChargeId) : IRequest<Result<StripeChargeDto>>;
