using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Queries.GetStripeCharges;

public class GetStripeChargesQueryHandler(IStripePaymentService stripePaymentService)
    : IRequestHandler<GetStripeChargesQuery, Result<StripeChargeListDto>>
{
    public async Task<Result<StripeChargeListDto>> Handle(
        GetStripeChargesQuery request,
        CancellationToken cancellationToken)
    {
        var list = await stripePaymentService.GetChargesAsync(
            request.Limit,
            request.StartingAfter,
            cancellationToken);

        return Result<StripeChargeListDto>.Success(list);
    }
}
