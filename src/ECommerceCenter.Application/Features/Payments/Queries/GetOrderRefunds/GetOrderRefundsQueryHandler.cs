using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Orders;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Queries.GetOrderRefunds;

public class GetOrderRefundsQueryHandler(
    IOrderRepository orderRepository,
    IRefundRepository refundRepository)
    : IRequestHandler<GetOrderRefundsQuery, Result<IReadOnlyList<RefundDto>>>
{
    public async Task<Result<IReadOnlyList<RefundDto>>> Handle(
        GetOrderRefundsQuery request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<IReadOnlyList<RefundDto>>.NotFound("Order", request.OrderId);

        var refunds = await refundRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        var dtos = refunds.Select(r => new RefundDto(
            r.Id,
            r.OrderId,
            r.Amount,
            r.CurrencyCode,
            r.Status.ToString(),
            r.Reason,
            r.ProviderRefundId,
            r.CreatedAt,
            r.UpdatedAt)).ToList();

        return Result<IReadOnlyList<RefundDto>>.Success(dtos);
    }
}
