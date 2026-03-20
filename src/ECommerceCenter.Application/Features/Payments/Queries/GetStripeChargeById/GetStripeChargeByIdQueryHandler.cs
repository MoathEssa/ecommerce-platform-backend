using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Queries.GetStripeChargeById;

public class GetStripeChargeByIdQueryHandler(IStripePaymentService stripePaymentService)
    : IRequestHandler<GetStripeChargeByIdQuery, Result<StripeChargeDto>>
{
    public async Task<Result<StripeChargeDto>> Handle(
        GetStripeChargeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var charge = await stripePaymentService.GetChargeByIdAsync(
            request.ChargeId,
            cancellationToken);

        if (charge is null)
            return Result<StripeChargeDto>.NotFound("Charge", request.ChargeId);

        return Result<StripeChargeDto>.Success(charge);
    }
}
