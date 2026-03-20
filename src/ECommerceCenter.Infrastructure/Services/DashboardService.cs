using ECommerceCenter.Application.Abstractions.DTOs.Dashboard;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Domain.Enums;
using ECommerceCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Services;

public class DashboardService(AppDbContext db, IStripePaymentService stripeService) : IDashboardService
{
    private static readonly List<OrderStatus> PaidStatuses =
    [
        OrderStatus.Paid,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Delivered,
        OrderStatus.PartiallyRefunded,
    ];

    public async Task<DashboardSummaryDto> GetSummaryAsync(int days, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 365);
        var now = DateTime.UtcNow;
        var periodStart = now.AddDays(-days);
        var prevPeriodStart = periodStart.AddDays(-days);

        // ── KPIs (revenue from Stripe) ──────────────────────────────────

        var stripeMetrics = await stripeService.GetRevenueMetricsAsync(periodStart, now, ct);
        var prevStripeMetrics = await stripeService.GetRevenueMetricsAsync(prevPeriodStart, periodStart, ct);

        var totalRevenue = stripeMetrics.TotalRevenue;
        var totalOrders = stripeMetrics.SuccessfulCharges;
        var avgOrderValue = stripeMetrics.AvgChargeAmount;
        var prevRevenue = prevStripeMetrics.TotalRevenue;
        var prevOrderCount = prevStripeMetrics.SuccessfulCharges;
        var prevAvg = prevStripeMetrics.AvgChargeAmount;

        var totalCustomers = await db.Orders
            .Where(o => o.CreatedAt >= periodStart)
            .Select(o => o.Email)
            .Distinct()
            .CountAsync(ct);

        var prevCustomers = await db.Orders
            .Where(o => o.CreatedAt >= prevPeriodStart && o.CreatedAt < periodStart)
            .Select(o => o.Email)
            .Distinct()
            .CountAsync(ct);

        var activeProducts = await db.Products
            .CountAsync(p => p.Status == ProductStatus.Active, ct);

        var totalRefunded = stripeMetrics.TotalRefunded;

        var pendingOrders = await db.Orders
            .CountAsync(o => o.Status == OrderStatus.PendingPayment, ct);

        var kpis = new KpiCardsDto(
            totalRevenue, prevRevenue,
            totalOrders, prevOrderCount,
            totalCustomers, prevCustomers,
            activeProducts,
            avgOrderValue, prevAvg,
            totalRefunded,
            pendingOrders);

        // ── Revenue chart (daily from Stripe) ─────────────────────────

        var revenueRows = stripeMetrics.DailyRevenue
            .Select(d => new RevenuePointDto(d.Date, d.Revenue, d.Charges))
            .ToList();

        // ── Order status breakdown ────────────────────────────────────────

        var statusBreakdown = (await db.Orders
            .Where(o => o.CreatedAt >= periodStart)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct))
            .Select(g => new OrderStatusBreakdownDto(g.Status.ToString(), g.Count))
            .ToList();

        // ── Top products ──────────────────────────────────────────────────

        var topProducts = await db.OrderItems
            .Where(oi => oi.Order.CreatedAt >= periodStart
                         && PaidStatuses.Contains(oi.Order.Status))
            .GroupBy(oi => new { oi.Variant.ProductId, oi.Variant.Product.Title })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Title,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal),
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync(ct);

        var topProductIds = topProducts.Select(p => p.ProductId).ToList();
        var images = await db.ProductImages
            .Where(i => topProductIds.Contains(i.ProductId) && i.VariantId == null)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Url = g.OrderBy(i => i.SortOrder).First().Url })
            .ToListAsync(ct);
        var imageMap = images.ToDictionary(i => i.ProductId, i => i.Url);

        var topProductDtos = topProducts.Select(p => new TopProductDto(
            p.ProductId, p.Title,
            imageMap.GetValueOrDefault(p.ProductId),
            p.QuantitySold, p.Revenue)).ToList();

        // ── Recent orders ─────────────────────────────────────────────────

        var recentOrders = await db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderDto(
                o.Id, o.OrderNumber, o.Email, o.Status.ToString(),
                o.Total, o.CurrencyCode, o.CreatedAt))
            .ToListAsync(ct);

        // ── Inventory alerts ──────────────────────────────────────────────

        const int lowStockThreshold = 10;

        var outOfStockCount = await db.InventoryItems
            .CountAsync(i => i.OnHand <= 0 && i.Variant.IsActive, ct);

        var lowStockItems = await db.InventoryItems
            .Where(i => i.OnHand > 0 && i.OnHand <= lowStockThreshold && i.Variant.IsActive)
            .OrderBy(i => i.OnHand)
            .Take(10)
            .Select(i => new LowStockItemDto(
                i.VariantId,
                i.Variant.Sku ?? $"V{i.VariantId}",
                i.Variant.Product.Title,
                i.OnHand))
            .ToListAsync(ct);

        var inventoryAlert = new InventoryAlertDto(lowStockItems.Count, outOfStockCount, lowStockItems);

        // ── Assemble ──────────────────────────────────────────────────────

        return new DashboardSummaryDto(
            kpis, revenueRows, statusBreakdown,
            topProductDtos, recentOrders, inventoryAlert);
    }
}
