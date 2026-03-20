using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.BulkImportCjProducts;

/// <summary>One row = one CJ variant being imported.</summary>
public record BulkImportVariantItem(
    /// <summary>CJ variant ID (<c>vid</c>). Stored as <c>ExternalSkuId</c> for deduplication.</summary>
    string CjVariantId,
    string VariantNameEn,
    string? ImageUrl,
    string SellPrice,
    string? CjPrice);

public record BulkImportResult(
    int Imported,
    int Skipped,
    IReadOnlyList<string> Errors);

public record BulkImportCjProductsCommand(
    /// <summary>CJ parent product ID — used to find or create the store product (created once).</summary>
    string CjProductId,
    /// <summary>Parent product name — used as the store product title when creating.</summary>
    string ProductNameEn,
    IReadOnlyList<BulkImportVariantItem> Items,
    string? CjCategoryId,
    string? OneCategoryName,
    string? TwoCategoryName,
    string? ThreeCategoryName,
    bool MakeActive = false) : IRequest<Result<BulkImportResult>>;
