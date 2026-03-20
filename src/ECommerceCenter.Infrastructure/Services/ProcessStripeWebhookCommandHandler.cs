using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Orders;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Application.Common.Settings;
using ECommerceCenter.Application.Features.Payments.Commands.ProcessWebhook;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Order = ECommerceCenter.Domain.Entities.Orders.Order;
using OrderItem = ECommerceCenter.Domain.Entities.Orders.OrderItem;
using OrderStatusHistory = ECommerceCenter.Domain.Entities.Orders.OrderStatusHistory;

namespace ECommerceCenter.Infrastructure.Services;

public class ProcessStripeWebhookCommandHandler(
    IPaymentProviderEventRepository providerEventRepository,
    IPaymentAttemptRepository paymentAttemptRepository,
    IRefundRepository refundRepository,
    IOrderRepository orderRepository,
    IGenericRepository<OrderItem> orderItemRepository,
    IInventoryItemRepository inventoryItemRepository,
    IOutboxMessageRepository outboxRepository,
    IEfUnitOfWork unitOfWork,
    IOptions<StripeSettings> stripeOptions,
    ILogger<ProcessStripeWebhookCommandHandler> logger)
    : IRequestHandler<ProcessStripeWebhookCommand, Result>
{
    private const string Provider = "Stripe";

    public async Task<Result> Handle(
        ProcessStripeWebhookCommand request,
        CancellationToken cancellationToken)
    {
        // ── Step 1: Verify Stripe signature ───────────────────────────────────
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                request.RawBody,
                request.StripeSignature,
                stripeOptions.Value.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            return Result.Failure(
                new Error("STRIPE_SIGNATURE_INVALID", "Webhook signature verification failed."),
                System.Net.HttpStatusCode.BadRequest);
        }

        // ── Step 2: Dedupe by EventId ─────────────────────────────────────────
        var alreadyProcessed = await providerEventRepository.ExistsByProviderEventIdAsync(
            Provider, stripeEvent.Id, cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Duplicate Stripe event {EventId} — skipping.", stripeEvent.Id);
            return Result.Success();
        }

        // ── Step 3: Route by event type ───────────────────────────────────────
        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceededAsync(stripeEvent, cancellationToken);
                break;

            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailedAsync(stripeEvent, cancellationToken);
                break;

            case "charge.refunded":
            case "charge.refund.updated":
                await HandleRefundUpdatedAsync(stripeEvent, cancellationToken);
                break;

            default:
                // Unrecognised event — store and return 200 to prevent Stripe retries
                logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                await StoreEventOnlyAsync(stripeEvent, null, cancellationToken);
                break;
        }

        return Result.Success();
    }

    // ── payment_intent.succeeded ──────────────────────────────────────────────

    private async Task HandlePaymentIntentSucceededAsync(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent
            ?? throw new InvalidOperationException("Expected PaymentIntent in event data.");

        await unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            // 4a. Insert event record (unique constraint prevents double-processing)
            await providerEventRepository.AddAsync(new PaymentProviderEvent
            {
                Provider = Provider,
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                RelatedIntentId = intent.Id,
                PayloadJson = stripeEvent.ToJson(),
                ReceivedAt = DateTime.UtcNow
            }, innerCt);
            await unitOfWork.SaveChangesAsync(innerCt);

            // 4b. Find and update PaymentAttempt
            var attempt = await paymentAttemptRepository.GetByProviderIntentIdAsync(
                Provider, intent.Id, innerCt);

            if (attempt is null)
            {
                logger.LogWarning("No PaymentAttempt found for Stripe intent {IntentId}.", intent.Id);
                return;
            }

            if (attempt.Status == PaymentAttemptStatus.Succeeded)
                return; // already succeeded — idempotent

            attempt.Status = PaymentAttemptStatus.Succeeded;
            attempt.UpdatedAt = DateTime.UtcNow;

            // 4c. Load and transition Order
            var order = await orderRepository.GetByIdAsync(attempt.OrderId, innerCt);
            if (order is null) return;

            if (order.Status == OrderStatus.Paid)
            {
                await unitOfWork.SaveChangesAsync(innerCt);
                return; // already paid — nothing to do
            }

            if (order.Status == OrderStatus.Canceled)
            {
                // Payment arrived after TTL-cancel — log and emit outbox for admin review
                logger.LogWarning(
                    "Payment succeeded for already-cancelled order {OrderId}.", order.Id);

                await outboxRepository.AddAsync(new OutboxMessage
                {
                    Type = OutboxMessageTypes.PaymentAfterCancellation,
                    PayloadJson = JsonSerializer.Serialize(new
                    {
                        orderId = order.Id,
                        orderNumber = order.OrderNumber,
                        intentId = intent.Id
                    }),
                    OccurredAt = DateTime.UtcNow
                }, innerCt);
                await unitOfWork.SaveChangesAsync(innerCt);
                return;
            }

            order.Status = OrderStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                FromStatus = OrderStatus.PendingPayment,
                ToStatus = OrderStatus.Paid,
                ChangedByType = ActorType.System,
                ChangedAt = DateTime.UtcNow,
                Note = $"Payment confirmed via Stripe webhook ({stripeEvent.Id})"
            });

            // 4d. Deduct inventory on payment success
            var orderItems = await orderItemRepository.FindAllAsync(
                oi => oi.OrderId == order.Id,
                cancellationToken: innerCt);
            foreach (var orderItem in orderItems)
            {
                var inv = await inventoryItemRepository.GetByVariantIdAsync(orderItem.VariantId, innerCt);
                if (inv is not null)
                {
                    inv.OnHand -= orderItem.Quantity;
                    inv.UpdatedAt = DateTime.UtcNow;
                }
            }

            // 4e. Outbox messages
            await outboxRepository.AddAsync(new OutboxMessage
            {
                Type = OutboxMessageTypes.PaymentSucceeded,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    orderId = order.Id,
                    intentId = intent.Id,
                    amount = attempt.Amount
                }),
                OccurredAt = DateTime.UtcNow
            }, innerCt);

            await outboxRepository.AddAsync(new OutboxMessage
            {
                Type = OutboxMessageTypes.OrderPaid,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    email = order.Email
                }),
                OccurredAt = DateTime.UtcNow
            }, innerCt);

            await outboxRepository.AddAsync(new OutboxMessage
            {
                Type = OutboxMessageTypes.InventoryCommitted,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    orderId = order.Id,
                    items = orderItems.Select(oi => new { oi.VariantId, oi.Quantity })
                }),
                OccurredAt = DateTime.UtcNow
            }, innerCt);

            await unitOfWork.SaveChangesAsync(innerCt);
        }, ct);
    }

    // ── payment_intent.payment_failed ─────────────────────────────────────────

    private async Task HandlePaymentIntentFailedAsync(Event stripeEvent, CancellationToken ct)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent
            ?? throw new InvalidOperationException("Expected PaymentIntent in event data.");

        await unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            await providerEventRepository.AddAsync(new PaymentProviderEvent
            {
                Provider = Provider,
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                RelatedIntentId = intent.Id,
                PayloadJson = stripeEvent.ToJson(),
                ReceivedAt = DateTime.UtcNow
            }, innerCt);

            var attempt = await paymentAttemptRepository.GetByProviderIntentIdAsync(
                Provider, intent.Id, innerCt);

            if (attempt is not null && attempt.Status != PaymentAttemptStatus.Failed)
            {
                attempt.Status = PaymentAttemptStatus.Failed;
                attempt.UpdatedAt = DateTime.UtcNow;

                await outboxRepository.AddAsync(new OutboxMessage
                {
                    Type = OutboxMessageTypes.PaymentFailed,
                    PayloadJson = JsonSerializer.Serialize(new
                    {
                        orderId = attempt.OrderId,
                        reason = intent.LastPaymentError?.Message ?? "Payment failed"
                    }),
                    OccurredAt = DateTime.UtcNow
                }, innerCt);
            }

            await unitOfWork.SaveChangesAsync(innerCt);
        }, ct);
    }

    // ── charge.refund.updated / charge.refunded ────────────────────────────────

    private async Task HandleRefundUpdatedAsync(Event stripeEvent, CancellationToken ct)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge?.Refunds?.Data is null || charge.Refunds.Data.Count == 0)
        {
            await StoreEventOnlyAsync(stripeEvent, charge?.PaymentIntentId, ct);
            return;
        }

        await unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            await providerEventRepository.AddAsync(new PaymentProviderEvent
            {
                Provider = Provider,
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                RelatedIntentId = charge.PaymentIntentId,
                PayloadJson = stripeEvent.ToJson(),
                ReceivedAt = DateTime.UtcNow
            }, innerCt);

            await unitOfWork.SaveChangesAsync(innerCt);

            foreach (var stripeRefund in charge.Refunds.Data)
            {
                var refund = await refundRepository.GetByProviderRefundIdAsync(stripeRefund.Id, innerCt);
                if (refund is null) continue;

                if (stripeRefund.Status == "succeeded" && refund.Status != RefundStatus.Succeeded)
                {
                    refund.Status = RefundStatus.Succeeded;
                    refund.UpdatedAt = DateTime.UtcNow;

                    // Re-check order refund status
                    var order = await orderRepository.GetByIdAsync(refund.OrderId, innerCt);
                    if (order is not null)
                    {
                        var totalRefunded = await refundRepository.GetTotalRefundedAmountAsync(
                            order.Id, innerCt);

                        var newStatus = totalRefunded >= order.Total
                            ? OrderStatus.Refunded
                            : OrderStatus.PartiallyRefunded;

                        if (order.Status != newStatus)
                        {
                            var fromStatus = order.Status;
                            order.Status = newStatus;
                            order.UpdatedAt = DateTime.UtcNow;

                            order.StatusHistory.Add(new OrderStatusHistory
                            {
                                OrderId = order.Id,
                                FromStatus = fromStatus,
                                ToStatus = newStatus,
                                ChangedByType = ActorType.System,
                                ChangedAt = DateTime.UtcNow,
                                Note = $"Refund confirmed via Stripe webhook ({stripeRefund.Id})"
                            });
                        }
                    }

                    await outboxRepository.AddAsync(new OutboxMessage
                    {
                        Type = OutboxMessageTypes.RefundCompleted,
                        PayloadJson = JsonSerializer.Serialize(new
                        {
                            orderId = refund.OrderId,
                            refundId = refund.Id,
                            amount = refund.Amount
                        }),
                        OccurredAt = DateTime.UtcNow
                    }, innerCt);
                }
                else if (stripeRefund.Status == "failed" && refund.Status != RefundStatus.Failed)
                {
                    refund.Status = RefundStatus.Failed;
                    refund.UpdatedAt = DateTime.UtcNow;

                    await outboxRepository.AddAsync(new OutboxMessage
                    {
                        Type = OutboxMessageTypes.RefundFailed,
                        PayloadJson = JsonSerializer.Serialize(new
                        {
                            orderId = refund.OrderId,
                            refundId = refund.Id,
                            reason = "Refund failed by payment provider"
                        }),
                        OccurredAt = DateTime.UtcNow
                    }, innerCt);
                }
            }

            await unitOfWork.SaveChangesAsync(innerCt);
        }, ct);
    }

    // ── Store event only (unhandled types) ────────────────────────────────────

    private async Task StoreEventOnlyAsync(Event stripeEvent, string? relatedIntentId, CancellationToken ct)
    {
        await providerEventRepository.AddAsync(new PaymentProviderEvent
        {
            Provider = Provider,
            EventId = stripeEvent.Id,
            EventType = stripeEvent.Type,
            RelatedIntentId = relatedIntentId,
            PayloadJson = stripeEvent.ToJson(),
            ReceivedAt = DateTime.UtcNow
        }, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
