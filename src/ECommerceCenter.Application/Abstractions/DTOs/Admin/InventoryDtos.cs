namespace ECommerceCenter.Application.Abstractions.DTOs.Admin;

public record InventoryListItemDto(
    int VariantId,
    string Sku,
    string ProductTitle,
    string? Options,
    int OnHand,
    int Available,
    string StockStatus,
    DateTime UpdatedAt);

public record InventoryDetailDto(
    int VariantId,
    string Sku,
    string ProductTitle,
    int OnHand,
    int Available,
    IReadOnlyList<AdjustmentDto> Adjustments);

public record AdjustmentDto(
    int Id,
    int Delta,
    string Reason,
    int? ActorId,
    DateTime CreatedAt);

public record InventoryAdjustmentResultDto(
    int VariantId,
    int OnHand,
    int Available,
    AdjustmentDto Adjustment);

public record CreateAdjustmentBody(int Delta, string Reason);
