using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.Settings;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerceCenter.Infrastructure.Services;

public class StripePaymentService(IOptions<StripeSettings> stripeOptions) : IStripePaymentService
{
    private readonly StripeSettings _settings = stripeOptions.Value;

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        long amountInSmallestUnit,
        string currency,
        string orderId,
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInSmallestUnit,
            Currency = currency.ToLowerInvariant(),
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = orderId,
                ["orderNumber"] = orderNumber
            },
            // Manual capture — we confirm on the client side via Stripe.js
            CaptureMethod = "automatic"
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new PaymentIntentResult(intent.Id, intent.ClientSecret);
    }

    public async Task<StripeRefundResult> CreateRefundAsync(
        string paymentIntentId,
        long amountInSmallestUnit,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = amountInSmallestUnit
        };

        var service = new RefundService();
        var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new StripeRefundResult(refund.Id, refund.Status);
    }

    public async Task<StripeChargeListDto> GetChargesAsync(
        int limit,
        string? startingAfter,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        var options = new ChargeListOptions { Limit = limit };
        if (!string.IsNullOrWhiteSpace(startingAfter))
            options.StartingAfter = startingAfter;

        var service = new ChargeService();
        var list = await service.ListAsync(options, cancellationToken: cancellationToken);

        return new StripeChargeListDto(
            list.Data.Select(MapCharge).ToList(),
            list.HasMore,
            list.Data.Count > 0 ? list.Data[^1].Id : null);
    }

    public async Task<StripeChargeDto?> GetChargeByIdAsync(
        string chargeId,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        try
        {
            var service = new ChargeService();
            var charge = await service.GetAsync(chargeId, cancellationToken: cancellationToken);
            return MapCharge(charge);
        }
        catch (StripeException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<StripeRevenueMetricsDto> GetRevenueMetricsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        var service = new ChargeService();
        var allCharges = new List<Charge>();
        string? startingAfter = null;

        // Paginate through all charges in the date range
        while (true)
        {
            var options = new ChargeListOptions
            {
                Limit = 100,
                Created = new DateRangeOptions
                {
                    GreaterThanOrEqual = from,
                    LessThan = to
                }
            };
            if (startingAfter != null)
                options.StartingAfter = startingAfter;

            var list = await service.ListAsync(options, cancellationToken: cancellationToken);
            allCharges.AddRange(list.Data);

            if (!list.HasMore || list.Data.Count == 0) break;
            startingAfter = list.Data[^1].Id;
        }

        var successful = allCharges.Where(c => c.Paid && c.Status == "succeeded").ToList();
        var totalRevenue = successful.Sum(c => c.AmountCaptured) / 100m;
        var totalRefunded = successful.Sum(c => c.AmountRefunded) / 100m;
        var count = successful.Count;
        var avg = count > 0 ? totalRevenue / count : 0;

        var dailyRevenue = successful
            .GroupBy(c => c.Created.Date)
            .OrderBy(g => g.Key)
            .Select(g => new StripeRevenueDayDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Sum(c => c.AmountCaptured) / 100m,
                g.Count()))
            .ToList();

        return new StripeRevenueMetricsDto(totalRevenue, totalRefunded, count, avg, dailyRevenue);
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static StripeChargeDto MapCharge(Charge c) => new(
        c.Id,
        c.Amount,
        c.AmountCaptured,
        c.AmountRefunded,
        c.BalanceTransactionId,
        c.BillingDetails is null ? null : new StripeBillingDetailsDto(
            c.BillingDetails.Address is null ? null : new StripeAddressDto(
                c.BillingDetails.Address.City,
                c.BillingDetails.Address.Country,
                c.BillingDetails.Address.Line1,
                c.BillingDetails.Address.Line2,
                c.BillingDetails.Address.PostalCode,
                c.BillingDetails.Address.State),
            c.BillingDetails.Email,
            c.BillingDetails.Name,
            c.BillingDetails.Phone),
        c.Captured,
        c.Created,
        c.Currency,
        c.CustomerId,
        c.Description,
        c.Disputed,
        c.FailureCode,
        c.FailureMessage,
        c.Paid,
        c.PaymentIntentId,
        c.PaymentMethod,
        c.PaymentMethodDetails is null ? null : new StripePaymentMethodDetailsDto(
            c.PaymentMethodDetails.Type,
            c.PaymentMethodDetails.Card is null ? null : new StripeCardDetailsDto(
                c.PaymentMethodDetails.Card.Brand,
                c.PaymentMethodDetails.Card.Country,
                (int?)c.PaymentMethodDetails.Card.ExpMonth,
                (int?)c.PaymentMethodDetails.Card.ExpYear,
                c.PaymentMethodDetails.Card.Fingerprint,
                c.PaymentMethodDetails.Card.Funding,
                c.PaymentMethodDetails.Card.Last4,
                c.PaymentMethodDetails.Card.Network)),
        c.ReceiptEmail,
        c.ReceiptNumber,
        c.ReceiptUrl,
        c.Refunded,
        c.StatementDescriptor,
        c.StatementDescriptorSuffix,
        c.Status,
        c.Outcome is null ? null : new StripeChargeOutcomeDto(
            c.Outcome.NetworkStatus,
            c.Outcome.Reason,
            c.Outcome.RiskLevel,
            c.Outcome.RiskScore,
            c.Outcome.SellerMessage,
            c.Outcome.Type));
}
