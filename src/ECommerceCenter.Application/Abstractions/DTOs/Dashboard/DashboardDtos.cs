namespace ECommerceCenter.Application.Abstractions.DTOs.Dashboard;

// ── Top-level summary ─────────────────────────────────────────────────────────

public record DashboardSummaryDto(
    KpiCardsDto Kpis,
    IReadOnlyList<RevenuePointDto> RevenueChart,
    IReadOnlyList<OrderStatusBreakdownDto> OrderStatusBreakdown,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<RecentOrderDto> RecentOrders,
    InventoryAlertDto InventoryAlert);

// ── KPI cards ─────────────────────────────────────────────────────────────────

public record KpiCardsDto(
    decimal TotalRevenue,
    decimal PrevPeriodRevenue,
    int TotalOrders,
    int PrevPeriodOrders,
    int TotalCustomers,
    int PrevPeriodCustomers,
    int ActiveProducts,
    decimal AvgOrderValue,
    decimal PrevPeriodAvgOrderValue,
    decimal TotalRefunded,
    int PendingOrders);

// ── Revenue over time ─────────────────────────────────────────────────────────

public record RevenuePointDto(string Date, decimal Revenue, int Orders);

// ── Orders by status ──────────────────────────────────────────────────────────

public record OrderStatusBreakdownDto(string Status, int Count);

// ── Best-selling products ─────────────────────────────────────────────────────

public record TopProductDto(
    int ProductId,
    string Title,
    string? ImageUrl,
    int QuantitySold,
    decimal Revenue);

// ── Recent orders ─────────────────────────────────────────────────────────────

public record RecentOrderDto(
    int Id,
    string OrderNumber,
    string Email,
    string Status,
    decimal Total,
    string CurrencyCode,
    DateTime CreatedAt);

// ── Inventory alerts ──────────────────────────────────────────────────────────

public record InventoryAlertDto(
    int LowStockCount,
    int OutOfStockCount,
    IReadOnlyList<LowStockItemDto> LowStockItems);

public record LowStockItemDto(
    int VariantId,
    string Sku,
    string ProductTitle,
    int Available);
