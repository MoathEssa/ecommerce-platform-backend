using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Orders;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;

namespace ECommerceCenter.Application.Features.Payments.Commands.CreateRefund;

public class CreateRefundCommandHandler(
    IOrderRepository orderRepository,
    IPaymentAttemptRepository paymentAttemptRepository,
    IRefundRepository refundRepository,
    IOutboxMessageRepository outboxRepository,
    IStripePaymentService stripePaymentService,
    IEfUnitOfWork unitOfWork,
    ILogger<CreateRefundCommandHandler> logger)
    : IRequestHandler<CreateRefundCommand, Result<RefundDto>>
{
    private static readonly OrderStatus[] RefundableStatuses =
    [
        OrderStatus.Paid,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.PartiallyRefunded
    ];

    public async Task<Result<RefundDto>> Handle(
        CreateRefundCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Validate eligibility
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result<RefundDto>.NotFound("Order", request.OrderId);

        if (!RefundableStatuses.Contains(order.Status))
            return Result<RefundDto>.Failure(
                Error.BusinessRule(OrderNotRefundable,
                    $"Order in status '{order.Status}' cannot be refunded."),
                System.Net.HttpStatusCode.BadRequest);

        // Calculate remaining refundable
        var totalRefunded = await refundRepository.GetTotalRefundedAmountAsync(order.Id, cancellationToken);
        var remainingRefundable = order.Total - totalRefunded;

        if (request.Amount > remainingRefundable)
            return Result<RefundDto>.Failure(
                Error.BusinessRule(RefundAmountExceedsRemaining,
                    $"Requested refund amount {request.Amount} exceeds remaining refundable {remainingRefundable}."),
                System.Net.HttpStatusCode.BadRequest);

        // Step 2: Find succeeded payment attempt
        var paymentAttempts = await paymentAttemptRepository.FindAllAsync(
            p => p.OrderId == order.Id && p.Status == PaymentAttemptStatus.Succeeded,
            cancellationToken: cancellationToken);
        var paymentAttempt = paymentAttempts.FirstOrDefault();

        if (paymentAttempt is null)
            return Result<RefundDto>.Failure(
                Error.BusinessRule(NoSucceededPayment,
                    "No succeeded payment found for this order."),
                System.Net.HttpStatusCode.BadRequest);

        // Step 3: Create Refund record
        var now = DateTime.UtcNow;
        var refund = new Refund
        {
            OrderId = order.Id,
            PaymentAttemptId = paymentAttempt.Id,
            Provider = "Stripe",
            Amount = request.Amount,
            CurrencyCode = order.CurrencyCode,
            Status = RefundStatus.Requested,
            Reason = request.Reason,
            CreatedAt = now
        };

        await refundRepository.AddAsync(refund, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 4: Call Stripe refund API
        try
        {
            var amountInSmallestUnit = (long)Math.Round(request.Amount * 100);
            var stripeResult = await stripePaymentService.CreateRefundAsync(
                paymentAttempt.ProviderIntentId, amountInSmallestUnit, cancellationToken);

            refund.ProviderRefundId = stripeResult.RefundId;

            if (stripeResult.Status == "succeeded")
            {
                refund.Status = RefundStatus.Succeeded;
                refund.UpdatedAt = now;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Stripe refund for order {OrderId}", order.Id);
            refund.Status = RefundStatus.Failed;
            refund.UpdatedAt = now;
        }

        // Step 5: Update order status
        var newTotalRefunded = totalRefunded + (refund.Status == RefundStatus.Succeeded ? request.Amount : 0);
        if (refund.Status == RefundStatus.Succeeded)
        {
            var fromStatus = order.Status;
            order.Status = newTotalRefunded >= order.Total
                ? OrderStatus.Refunded
                : OrderStatus.PartiallyRefunded;
            order.UpdatedAt = now;

            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                FromStatus = fromStatus,
                ToStatus = order.Status,
                ChangedBy = request.ActorId,
                ChangedByType = ActorType.Admin,
                ChangedAt = now,
                Note = request.Reason ?? "Admin-initiated refund"
            });
        }

        // Step 6: Outbox + audit
        await outboxRepository.AddAsync(new OutboxMessage
        {
            Type = OutboxMessageTypes.RefundIssued,
            PayloadJson = JsonSerializer.Serialize(new
            {
                orderId = order.Id,
                refundId = refund.Id,
                amount = refund.Amount,
                reason = refund.Reason
            }),
            OccurredAt = now
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RefundDto>.Success(new RefundDto(
            refund.Id,
            refund.OrderId,
            refund.Amount,
            refund.CurrencyCode,
            refund.Status.ToString(),
            refund.Reason,
            refund.ProviderRefundId,
            refund.CreatedAt,
            refund.UpdatedAt));
    }
}
