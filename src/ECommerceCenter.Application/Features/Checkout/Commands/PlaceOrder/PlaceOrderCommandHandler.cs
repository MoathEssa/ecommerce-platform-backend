using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Orders;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Application.Common.Settings;
using ECommerceCenter.Domain.Entities.Coupons;
using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;

namespace ECommerceCenter.Application.Features.Checkout.Commands.PlaceOrder;

public class PlaceOrderCommandHandler(
    IIdempotencyKeyRepository idempotencyKeyRepository,
    IProductVariantRepository variantRepository,
    IInventoryItemRepository inventoryItemRepository,
    IOrderRepository orderRepository,
    IPaymentAttemptRepository paymentAttemptRepository,
    IOutboxMessageRepository outboxRepository,
    ICouponRepository couponRepository,
    ICouponEvaluator couponEvaluator,
    IStripePaymentService stripePaymentService,
    IEfUnitOfWork unitOfWork,
    IOptions<StoreSettings> storeOptions,
    ILogger<PlaceOrderCommandHandler> logger)
    : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResponse>>
{
    private const string CheckoutRoute = "POST:/api/v1/checkout";

    public async Task<Result<PlaceOrderResponse>> Handle(
        PlaceOrderCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Idempotency check
        var existing = await idempotencyKeyRepository.GetByKeyAndRouteAsync(
            request.IdempotencyKey, CheckoutRoute, cancellationToken);

        if (existing is not null)
        {
            if (existing.RequestHash != request.RequestBodyHash)
                return Result<PlaceOrderResponse>.Conflict(
                    "The same idempotency key was used with a different request body. Please generate a new key.");

            var cached = JsonSerializer.Deserialize<PlaceOrderResponse>(existing.ResponseJson);
            if (cached is not null)
                return Result<PlaceOrderResponse>.Success(cached);

            // Cached JSON could not be deserialized (schema drift / corruption) — fall through
            // and process the request as new. This is safe: the DB unique constraint on
            // (Key, Route) prevents a duplicate IdempotencyKey row from being inserted.
            logger.LogWarning(
                "Idempotency cache hit for key {Key} but response JSON could not be deserialized. Processing as new request.",
                request.IdempotencyKey);
        }

        // Step 2: Bulk-load and validate variants (single query, no N+1)
        var distinctVariantIds = request.Items.Select(i => i.VariantId).Distinct().ToList();
        var variantMap = await variantRepository.GetWithProductsAsync(distinctVariantIds, cancellationToken);

        foreach (var variantId in distinctVariantIds)
        {
            if (!variantMap.TryGetValue(variantId, out var variant) || !variant.IsActive)
                return Result<PlaceOrderResponse>.Failure(
                    new Error(VariantNotFound.ToString(),
                        $"Variant {variantId} was not found or is inactive."),
                    System.Net.HttpStatusCode.BadRequest);

            if (variant.Product.Status != ProductStatus.Active)
                return Result<PlaceOrderResponse>.Failure(
                    new Error(ProductNotActive.ToString(),
                        $"Product for variant {variantId} is not available."),
                    System.Net.HttpStatusCode.BadRequest);
        }

        // Step 2b: All variants must share the same currency
        var currencies = variantMap.Values
            .Select(v => v.CurrencyCode)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .ToList();

        if (currencies.Count == 0)
            return Result<PlaceOrderResponse>.Failure(
                new Error(VariantNotFound.ToString(), "No valid currency code found on the requested variants."),
                System.Net.HttpStatusCode.BadRequest);

        if (currencies.Count > 1)
            return Result<PlaceOrderResponse>.BusinessRuleViolation(
                CurrencyMismatch,
                "All items in an order must share the same currency code.");

        var currency = currencies[0];

        // Step 3: Aggregate quantities + re-validate max per line (after grouping)
        var aggregated = request.Items
            .GroupBy(i => i.VariantId)
            .Select(g => (VariantId: g.Key, Quantity: g.Sum(x => x.Quantity)))
            .ToList();

        var overLimit = aggregated.FirstOrDefault(x => x.Quantity > storeOptions.Value.MaxQuantityPerLine);
        if (overLimit != default)
            return Result<PlaceOrderResponse>.BusinessRuleViolation(
                ExceedsMaxQuantity,
                $"Variant {overLimit.VariantId}: maximum order quantity per line is {storeOptions.Value.MaxQuantityPerLine}.");

        // Step 4: Calculate prices
        decimal subtotal = aggregated.Sum(x => variantMap[x.VariantId].BasePrice * x.Quantity);

        var billingCountry = (request.BillingAddress ?? request.ShippingAddress).Country.ToUpperInvariant();
        var taxRate   = storeOptions.Value.GetTaxRate(billingCountry);
        var taxTotal  = Math.Round(subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
        const decimal shippingTotal = 0m; // MVP: free shipping

        // Step 5: Evaluate coupon (loaded exactly once inside the evaluator)
        decimal discountTotal   = 0m;
        int?    appliedCouponId = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var evalItems = aggregated
                .Select(x => new CartItemForEvaluation(x.VariantId, variantMap[x.VariantId].BasePrice * x.Quantity))
                .ToList();

            var evalResult = await couponEvaluator.EvaluateAsync(
                request.CouponCode, evalItems, subtotal, request.UserId, null, cancellationToken);

            if (!evalResult.IsValid)
                return Result<PlaceOrderResponse>.BusinessRuleViolation(
                    evalResult.FailureCode!.Value, evalResult.FailureReason!);

            discountTotal   = evalResult.DiscountAmount;
            appliedCouponId = evalResult.CouponId; 
        }

        var total = subtotal - discountTotal + taxTotal + shippingTotal;

        if (total < 0)
            return Result<PlaceOrderResponse>.BusinessRuleViolation(
                NegativeOrderTotal, "The calculated order total is negative. Please check your coupon.");

        // Capture once — reused for all entity timestamps and order number year
        var now       = DateTime.UtcNow;

        // Steps 6-8: Atomic transaction
        PlaceOrderResponse?         response  = null;
        Result<PlaceOrderResponse>? earlyExit = null;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                // 6a. Bulk-load inventory WITH tracking (single query, sorted for deadlock prevention)
                var sortedItems  = aggregated.OrderBy(x => x.VariantId).ToList();
                
                var inventoryMap = await inventoryItemRepository.GetByVariantIdsAsync(
                    sortedItems.Select(x => x.VariantId).ToList(), ct);

                // 6b. Validate stock availability
                foreach (var (variantId, quantity) in sortedItems)
                {
                    if (!inventoryMap.TryGetValue(variantId, out var inv))
                        throw new InvalidOperationException($"No inventory record for variant {variantId}.");

                    if (inv.OnHand < quantity)
                    {
                        earlyExit = Result<PlaceOrderResponse>.BusinessRuleViolation(
                            InsufficientStock,
                            $"Insufficient stock for {variantMap[variantId].Sku}. Available: {inv.OnHand}.");
                        return; 
                    }
                }

                if (earlyExit is not null) return;

                // 6c. Insert Order
                var order = new Order
                {
                    OrderNumber   = string.Empty, // assigned after Id is materialised
                    UserId        = request.UserId,
                    Email         = request.Email ?? string.Empty,
                    Status        = OrderStatus.PendingPayment,
                    CurrencyCode  = currency,
                    Subtotal      = subtotal,
                    DiscountTotal = discountTotal,
                    TaxTotal      = taxTotal,
                    ShippingTotal = shippingTotal,
                    Total         = total,
                    CouponCode    = string.IsNullOrWhiteSpace(request.CouponCode)
                                        ? null
                                        : request.CouponCode.Trim().ToUpperInvariant(),
                    CreatedAt     = now
                };
                await orderRepository.AddAsync(order, ct);
                await unitOfWork.SaveChangesAsync(ct); 

                order.OrderNumber = $"ORD-{now.Year}-{order.Id:D6}";

                // 6d. OrderItems
                foreach (var (variantId, quantity) in sortedItems)
                {
                    var v = variantMap[variantId];
                    order.Items.Add(new OrderItem
                    {
                        OrderId      = order.Id,
                        VariantId    = variantId,
                        SkuSnapshot  = v.Sku ?? $"VARIANT-{variantId}",
                        NameSnapshot = BuildNameSnapshot(v.Product.Title, v.OptionsJson),
                        UnitPrice    = v.BasePrice,
                        Quantity     = quantity,
                        LineTotal    = v.BasePrice * quantity,
                        CreatedAt    = now
                    });
                }

                // 6e. OrderAddresses
                order.Addresses.Add(MapAddress(order.Id, AddressType.Shipping, request.ShippingAddress, now));
                order.Addresses.Add(MapAddress(order.Id, AddressType.Billing,
                    request.BillingAddress ?? request.ShippingAddress, now));

                // 6f. Initial status-history entry (FromStatus = null — no prior state)
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId       = order.Id,
                    FromStatus    = null,
                    ToStatus      = OrderStatus.PendingPayment,
                    ChangedByType = request.UserId.HasValue ? ActorType.User : ActorType.System,
                    ChangedBy     = request.UserId,
                    ChangedAt     = now,
                    Note          = "Order created at checkout"
                });

                // 6h. CouponUsage — reload with tracking to update UsedCount atomically
                if (appliedCouponId.HasValue)
                {
                    var couponEntity = await couponRepository.GetByCodeWithRulesAsync(
                        request.CouponCode!.Trim().ToUpperInvariant(), ct);

                    if (couponEntity is not null)
                    {
                        couponEntity.UsedCount += 1;
                        order.CouponUsage = new CouponUsage
                        {
                            CouponId        = couponEntity.Id,
                            OrderId         = order.Id,
                            UserId          = request.UserId,
                            DiscountApplied = discountTotal,
                            UsedAt          = now
                        };
                    }
                }

                // 6i. Call Stripe BEFORE final commit — failure rolls back everything
                var amountInSmallestUnit = (long)Math.Round(total * 100);
                var intentResult = await stripePaymentService.CreatePaymentIntentAsync(
                    amountInSmallestUnit, currency, order.Id.ToString(), order.OrderNumber, ct);

                // 6j. PaymentAttempt
                await paymentAttemptRepository.AddAsync(new PaymentAttempt
                {
                    OrderId          = order.Id,
                    Provider         = "Stripe",
                    ProviderIntentId = intentResult.IntentId,
                    Status           = PaymentAttemptStatus.Created,
                    Amount           = total,
                    CurrencyCode     = currency,
                    CreatedAt        = now
                }, ct);

                // 6j. OutboxMessages (type strings from constants — no magic literals)
                foreach (var (msgType, msgPayload) in new[]
                {
                    (OutboxMessageTypes.OrderPlaced,
                        (object)new { orderId = order.Id, orderNumber = order.OrderNumber, email = order.Email }),
                    (OutboxMessageTypes.PaymentIntentCreated,
                        (object)new { orderId = order.Id, intentId = intentResult.IntentId })
                })
                {
                    await outboxRepository.AddAsync(new OutboxMessage
                    {
                        Type        = msgType,
                        PayloadJson = JsonSerializer.Serialize(msgPayload),
                        OccurredAt  = now
                    }, ct);
                }

                // 6k. IdempotencyKey — DB unique constraint (Key, Route) is the race-condition safeguard
                var responseDto = BuildResponse(order, intentResult,
                    subtotal, discountTotal, taxTotal, shippingTotal, total, request.Items);

                await idempotencyKeyRepository.AddAsync(new IdempotencyKey
                {
                    Key          = request.IdempotencyKey,
                    Route        = CheckoutRoute,
                    RequestHash  = request.RequestBodyHash,
                    ResponseJson = JsonSerializer.Serialize(responseDto),
                    StatusCode   = 200,
                    CreatedAt    = now,
                    ExpiresAt    = now.AddDays(7)
                }, ct);

                await unitOfWork.SaveChangesAsync(ct);
                response = responseDto;

        }, cancellationToken);

        if (earlyExit is not null)
            return earlyExit;

        return Result<PlaceOrderResponse>.Success(response!);
    }

    // Helpers

    private static PlaceOrderResponse BuildResponse(
        Order order,
        PaymentIntentResult intentResult,
        decimal subtotal,
        decimal discountTotal,
        decimal taxTotal,
        decimal shippingTotal,
        decimal total,
        IReadOnlyList<CheckoutItemDto> items) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Total,
            order.CurrencyCode,
            order.Status.ToString(),
            new PlaceOrderPaymentDto("Stripe", intentResult.ClientSecret, intentResult.IntentId),
            new PlaceOrderSummaryDto(
                items.Sum(i => i.Quantity),
                subtotal,
                discountTotal,
                taxTotal,
                shippingTotal,
                total));

    private static OrderAddress MapAddress(
        int orderId, AddressType type, CheckoutAddressDto dto, DateTime now) =>
        new()
        {
            OrderId    = orderId,
            Type       = type,
            FullName   = dto.FullName,
            Phone      = dto.Phone,
            Line1      = dto.Line1,
            Line2      = dto.Line2,
            City       = dto.City,
            Region     = dto.Region,
            PostalCode = dto.PostalCode,
            Country    = dto.Country,
            CreatedAt  = now
        };

    private string BuildNameSnapshot(string productTitle, string optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson) || optionsJson == "{}")
            return productTitle;
        try
        {
            var opts = JsonSerializer.Deserialize<Dictionary<string, string>>(optionsJson);
            if (opts is null || opts.Count == 0) return productTitle;
            return $"{productTitle} — {string.Join(", ", opts.Select(kv => $"{kv.Key}: {kv.Value}"))}";
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                "Failed to parse optionsJson for product '{Title}'. Falling back to title only.",
                productTitle);
            return productTitle;
        }
    }

}
