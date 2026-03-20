using System.Text.Json.Serialization;

namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping.Models;

// ── Product List V2 response ──────────────────────────────────────────────────

internal sealed record CjProductListV2Data(
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("pageNumber")] int PageNumber,
    [property: JsonPropertyName("totalRecords")] long TotalRecords,
    [property: JsonPropertyName("totalPages")] int TotalPages,
    [property: JsonPropertyName("content")] List<CjProductSearchContent>? Content);

internal sealed record CjProductSearchContent(
    [property: JsonPropertyName("productList")] List<CjProductItem>? ProductList,
    [property: JsonPropertyName("relatedCategoryList")] List<CjRelatedCategory>? RelatedCategoryList,
    [property: JsonPropertyName("keyWord")] string? KeyWord,
    [property: JsonPropertyName("keyWordOld")] string? KeyWordOld);

internal sealed record CjRelatedCategory(
    [property: JsonPropertyName("categoryId")] string? CategoryId,
    [property: JsonPropertyName("categoryName")] string? CategoryName);

internal sealed record CjProductItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("nameEn")] string NameEn,
    [property: JsonPropertyName("sku")] string? Sku,
    [property: JsonPropertyName("spu")] string? Spu,
    [property: JsonPropertyName("bigImage")] string? BigImage,
    [property: JsonPropertyName("sellPrice")] string? SellPrice,
    [property: JsonPropertyName("nowPrice")] string? NowPrice,
    [property: JsonPropertyName("discountPrice")] string? DiscountPrice,
    [property: JsonPropertyName("discountPriceRate")] string? DiscountPriceRate,
    [property: JsonPropertyName("listedNum")] int? ListedNum,
    [property: JsonPropertyName("categoryId")] string? CategoryId,
    [property: JsonPropertyName("threeCategoryName")] string? ThreeCategoryName,
    [property: JsonPropertyName("twoCategoryId")] string? TwoCategoryId,
    [property: JsonPropertyName("twoCategoryName")] string? TwoCategoryName,
    [property: JsonPropertyName("oneCategoryId")] string? OneCategoryId,
    [property: JsonPropertyName("oneCategoryName")] string? OneCategoryName,
    [property: JsonPropertyName("addMarkStatus")] int? AddMarkStatus,
    [property: JsonPropertyName("isVideo")] int? IsVideo,
    [property: JsonPropertyName("productType")] string? ProductType,
    [property: JsonPropertyName("supplierName")] string? SupplierName,
    [property: JsonPropertyName("createAt")] long? CreateAt,
    [property: JsonPropertyName("warehouseInventoryNum")] long? WarehouseInventoryNum,
    [property: JsonPropertyName("totalVerifiedInventory")] int? TotalVerifiedInventory,
    [property: JsonPropertyName("totalUnVerifiedInventory")] int? TotalUnVerifiedInventory,
    [property: JsonPropertyName("verifiedWarehouse")] int? VerifiedWarehouse,
    [property: JsonPropertyName("customization")] int? Customization,
    [property: JsonPropertyName("hasCECertification")] int? HasCECertification,
    [property: JsonPropertyName("isCollect")] int? IsCollect,
    [property: JsonPropertyName("myProduct")] bool? MyProduct,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("deliveryCycle")] string? DeliveryCycle,
    [property: JsonPropertyName("saleStatus")] string? SaleStatus,
    [property: JsonPropertyName("isPersonalized")] int? IsPersonalized);

// ── Product Variant response ──────────────────────────────────────────────────

internal sealed record CjVariantItem(
    [property: JsonPropertyName("vid")] string Vid,
    [property: JsonPropertyName("pid")] string Pid,
    [property: JsonPropertyName("variantNameEn")] string? VariantNameEn,
    [property: JsonPropertyName("variantImage")] string? VariantImage,
    [property: JsonPropertyName("variantSku")] string? VariantSku,
    [property: JsonPropertyName("variantKey")] string? VariantKey,
    [property: JsonPropertyName("variantStandard")] string? VariantStandard,
    [property: JsonPropertyName("variantSellPrice")] decimal? VariantSellPrice,
    [property: JsonPropertyName("variantSugSellPrice")] decimal? VariantSugSellPrice,
    [property: JsonPropertyName("variantWeight")] decimal? VariantWeight);

// ── Product Detail (query by pid) ─────────────────────────────────────────────

internal sealed record CjProductDetailData(
    [property: JsonPropertyName("pid")] string Pid,
    [property: JsonPropertyName("productNameEn")] string? ProductNameEn,
    [property: JsonPropertyName("productSku")] string? ProductSku,
    [property: JsonPropertyName("bigImage")] string? BigImage,
    [property: JsonPropertyName("productImageSet")] List<string>? ProductImageSet,
    [property: JsonPropertyName("productWeight")] string? ProductWeight,
    [property: JsonPropertyName("categoryId")] string? CategoryId,
    [property: JsonPropertyName("categoryName")] string? CategoryName,
    [property: JsonPropertyName("materialNameEn")] string? MaterialNameEn,
    [property: JsonPropertyName("materialNameEnSet")] List<string>? MaterialNameEnSet,
    [property: JsonPropertyName("productKeyEn")] string? ProductKeyEn,
    [property: JsonPropertyName("sellPrice")] string? SellPrice,
    [property: JsonPropertyName("suggestSellPrice")] string? SuggestSellPrice,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("variants")] List<CjProductDetailVariant>? Variants,
    [property: JsonPropertyName("packingWeight")] string? PackingWeight,
    [property: JsonPropertyName("entryNameEn")] string? EntryNameEn,
    [property: JsonPropertyName("listedNum")] int? ListedNum,
    [property: JsonPropertyName("status")] string? Status);

internal sealed record CjProductDetailVariant(
    [property: JsonPropertyName("vid")] string Vid,
    [property: JsonPropertyName("pid")] string Pid,
    [property: JsonPropertyName("variantNameEn")] string? VariantNameEn,
    [property: JsonPropertyName("variantImage")] string? VariantImage,
    [property: JsonPropertyName("variantSku")] string? VariantSku,
    [property: JsonPropertyName("variantKey")] string? VariantKey,
    [property: JsonPropertyName("variantStandard")] string? VariantStandard,
    [property: JsonPropertyName("variantSellPrice")] decimal? VariantSellPrice,
    [property: JsonPropertyName("variantSugSellPrice")] decimal? VariantSugSellPrice,
    [property: JsonPropertyName("variantWeight")] decimal? VariantWeight,
    [property: JsonPropertyName("variantLength")] int? VariantLength,
    [property: JsonPropertyName("variantWidth")] int? VariantWidth,
    [property: JsonPropertyName("variantHeight")] int? VariantHeight);

// ── Variant Stock (queryByVid) ────────────────────────────────────────────────

internal sealed record CjVariantStockItem(
    [property: JsonPropertyName("vid")] string? Vid,
    [property: JsonPropertyName("areaEn")] string? AreaEn,
    [property: JsonPropertyName("storageNum")] int StorageNum,
    [property: JsonPropertyName("totalInventoryNum")] int TotalInventoryNum);

// ── Product Comments (v1/product/comments) ────────────────────────────────────
// Note: this older endpoint uses "success"/"code":0 for success, unlike the
// newer endpoints which use "result":true.

internal sealed record CjCommentsApiResponse<T>(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("success")] bool? Success,
    [property: JsonPropertyName("result")] bool? Result,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("data")] T? Data);

internal sealed record CjProductCommentsData(
    [property: JsonPropertyName("pageNum")] int? PageNum,
    [property: JsonPropertyName("pageSize")] int? PageSize,
    [property: JsonPropertyName("total")] int? Total,
    [property: JsonPropertyName("list")] List<CjCommentItem>? List);

internal sealed record CjCommentItem(
    [property: JsonPropertyName("commentId")] long CommentId,
    [property: JsonPropertyName("pid")] string? Pid,
    [property: JsonPropertyName("comment")] string? Comment,
    [property: JsonPropertyName("commentDate")] string? CommentDate,
    [property: JsonPropertyName("commentUser")] string? CommentUser,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("commentUrls")] List<string>? CommentUrls,
    [property: JsonPropertyName("countryCode")] string? CountryCode,
    [property: JsonPropertyName("flagIconUrl")] string? FlagIconUrl);
