using ECommerceCenter.Application.Abstractions.DTOs.Payments;

namespace ECommerceCenter.Application.Abstractions.Services;

/// <summary>Result of creating a Stripe PaymentIntent.</summary>
public record PaymentIntentResult(
    string IntentId,
    string ClientSecret);

/// <summary>Result of creating a Stripe refund.</summary>
public record StripeRefundResult(string RefundId, string Status);

public interface IStripePaymentService
{
    /// <summary>Creates a PaymentIntent on Stripe. Amount is in the smallest currency unit (halalas for SAR).</summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        long amountInSmallestUnit,
        string currency,
        string orderId,
        string orderNumber,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a refund on Stripe for the given PaymentIntent.</summary>
    Task<StripeRefundResult> CreateRefundAsync(
        string paymentIntentId,
        long amountInSmallestUnit,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a paginated list of Stripe charges.</summary>
    Task<StripeChargeListDto> GetChargesAsync(
        int limit,
        string? startingAfter,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single Stripe charge by its ID, or null if not found.</summary>
    Task<StripeChargeDto?> GetChargeByIdAsync(
        string chargeId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns aggregated revenue metrics from Stripe charges for a date range.</summary>
    Task<StripeRevenueMetricsDto> GetRevenueMetricsAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}

/// <summary>Aggregated Stripe revenue data for the dashboard.</summary>
public record StripeRevenueMetricsDto(
    decimal TotalRevenue,
    decimal TotalRefunded,
    int SuccessfulCharges,
    decimal AvgChargeAmount,
    List<StripeRevenueDayDto> DailyRevenue);

public record StripeRevenueDayDto(string Date, decimal Revenue, int Charges);
