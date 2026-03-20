using System.Net.Http.Json;
using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping.Models;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping;

/// <summary>
/// Adapts the CJDropshipping REST API product endpoints to <see cref="ICjProductService"/>.
/// Uses the named <see cref="HttpClient"/> registered as "CjDropshipping".
/// </summary>
public sealed class CjDropshippingProductService(
    IHttpClientFactory httpClientFactory,
    ICjAccessTokenProvider tokenProvider,
    ILogger<CjDropshippingProductService> logger) : ICjProductService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<CjCategoryNodeDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var token  = await tokenProvider.GetCurrentTokenAsync(ct);
        var client = httpClientFactory.CreateClient("CjDropshipping");

        using var request = new HttpRequestMessage(HttpMethod.Get, "v1/product/getCategory");
        request.Headers.Add("CJ-Access-Token", token);

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var envelope = JsonSerializer.Deserialize<CjCategoryListResponse>(json, JsonOptions);

        if (envelope is null || !envelope.Result || envelope.Data is null)
        {
            logger.LogWarning(
                "CJDropshipping category list returned an unexpected response. Code={Code} Message={Message}",
                envelope?.Code, envelope?.Message);
            return [];
        }

        return MapToTree(envelope.Data);
    }

    // ── Private mapping ───────────────────────────────────────────────────────

    private static List<CjCategoryNodeDto> MapToTree(List<CjFirstLevelCategory> raw)
    {
        var result = new List<CjCategoryNodeDto>(raw.Count);

        foreach (var first in raw)
        {
            var secondNodes = new List<CjCategoryNodeDto>(
                first.CategoryFirstList?.Count ?? 0);

            foreach (var second in first.CategoryFirstList ?? [])
            {
                var thirdNodes = (second.CategorySecondList ?? [])
                    .Select(t => new CjCategoryNodeDto(
                        CategoryId: t.CategoryId,
                        Name:       t.CategoryName,
                        Level:      3,
                        Children:   []))
                    .ToList();

                secondNodes.Add(new CjCategoryNodeDto(
                    CategoryId: second.CategorySecondId,
                    Name:       second.CategorySecondName,
                    Level:      2,
                    Children:   thirdNodes));
            }

            result.Add(new CjCategoryNodeDto(
                CategoryId: first.CategoryFirstId,
                Name:       first.CategoryFirstName,
                Level:      1,
                Children:   secondNodes));
        }

        return result;
    }

    // ── Product List V2 ───────────────────────────────────────────────────────

    public async Task<CjProductListResult> SearchProductsAsync(
        CjProductSearchParams searchParams,
        CancellationToken ct = default)
    {
        var token  = await tokenProvider.GetCurrentTokenAsync(ct);
        var client = httpClientFactory.CreateClient("CjDropshipping");

        var qs = new List<string>
        {
            $"page={searchParams.Page}",
            $"size={searchParams.Size}"
        };

        if (!string.IsNullOrWhiteSpace(searchParams.KeyWord))
            qs.Add($"keyWord={Uri.EscapeDataString(searchParams.KeyWord)}");
        if (!string.IsNullOrWhiteSpace(searchParams.CategoryId))
            qs.Add($"categoryId={Uri.EscapeDataString(searchParams.CategoryId)}");
        if (!string.IsNullOrWhiteSpace(searchParams.CountryCode))
            qs.Add($"countryCode={Uri.EscapeDataString(searchParams.CountryCode)}");
        if (searchParams.StartSellPrice.HasValue)
            qs.Add($"startSellPrice={searchParams.StartSellPrice.Value}");
        if (searchParams.EndSellPrice.HasValue)
            qs.Add($"endSellPrice={searchParams.EndSellPrice.Value}");
        if (searchParams.AddMarkStatus.HasValue)
            qs.Add($"addMarkStatus={searchParams.AddMarkStatus.Value}");
        if (searchParams.ProductType.HasValue)
            qs.Add($"productType={searchParams.ProductType.Value}");
        if (!string.IsNullOrWhiteSpace(searchParams.Sort))
            qs.Add($"sort={Uri.EscapeDataString(searchParams.Sort)}");
        if (searchParams.OrderBy.HasValue)
            qs.Add($"orderBy={searchParams.OrderBy.Value}");

        var url = $"v1/product/listV2?{string.Join("&", qs)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("CJ-Access-Token", token);

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var envelope = JsonSerializer.Deserialize<CjApiResponse<CjProductListV2Data>>(json, JsonOptions);

        if (envelope is null || !envelope.Result || envelope.Data is null)
        {
            logger.LogWarning(
                "CJDropshipping product list V2 returned an unexpected response. Code={Code} Message={Message}",
                envelope?.Code, envelope?.Message);
            return new CjProductListResult([], 1, searchParams.Size, 0, 0);
        }

        var data = envelope.Data;
        var products = data.Content?
            .SelectMany(c => c.ProductList ?? [])
            .Select(p => new CjProductListItemDto(
                Id:                  p.Id,
                NameEn:              p.NameEn,
                Sku:                 p.Sku,
                BigImage:            p.BigImage,
                SellPrice:           p.SellPrice,
                DiscountPrice:       p.DiscountPrice,
                DiscountPriceRate:   p.DiscountPriceRate,
                ListedNum:           p.ListedNum,
                CategoryId:          p.CategoryId,
                ThreeCategoryName:   p.ThreeCategoryName,
                TwoCategoryName:     p.TwoCategoryName,
                OneCategoryName:     p.OneCategoryName,
                FreeShipping:        p.AddMarkStatus == 1,
                ProductType:         p.ProductType,
                SupplierName:        p.SupplierName,
                CreateAt:            p.CreateAt,
                WarehouseInventoryNum: p.WarehouseInventoryNum,
                DeliveryCycle:       p.DeliveryCycle))
            .ToList() ?? [];

        return new CjProductListResult(
            products,
            data.PageNumber,
            data.PageSize,
            data.TotalRecords,
            data.TotalPages);
    }

    // ── Product Variants ──────────────────────────────────────────────────────

    public async Task<List<CjProductVariantDto>> GetProductVariantsAsync(
        string pid,
        string? countryCode = null,
        CancellationToken ct = default)
    {
        var token  = await tokenProvider.GetCurrentTokenAsync(ct);
        var client = httpClientFactory.CreateClient("CjDropshipping");

        var qs = new List<string> { $"pid={Uri.EscapeDataString(pid)}" };
        if (!string.IsNullOrWhiteSpace(countryCode))
            qs.Add($"countryCode={Uri.EscapeDataString(countryCode)}");

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"v1/product/variant/query?{string.Join("&", qs)}");
        request.Headers.Add("CJ-Access-Token", token);

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var envelope = JsonSerializer.Deserialize<CjApiResponse<List<CjVariantItem>>>(json, JsonOptions);

        if (envelope is null || !envelope.Result || envelope.Data is null)
        {
            logger.LogWarning(
                "CJDropshipping variant query returned unexpected response. Code={Code} Message={Message}",
                envelope?.Code, envelope?.Message);
            return [];
        }

        return envelope.Data
            .Select(v => new CjProductVariantDto(
                Vid:                v.Vid,
                Pid:                v.Pid,
                VariantNameEn:      v.VariantNameEn,
                VariantImage:       v.VariantImage,
                VariantSku:         v.VariantSku,
                VariantKey:         v.VariantKey,
                VariantStandard:    v.VariantStandard,
                VariantSellPrice:   v.VariantSellPrice,
                VariantSugSellPrice: v.VariantSugSellPrice,
                VariantWeight:      v.VariantWeight))
            .ToList();
    }

    // ── Product Detail ────────────────────────────────────────────────────────

    public async Task<CjProductDetailDto?> GetProductDetailAsync(
        string pid, CancellationToken ct = default)
    {
        var token  = await tokenProvider.GetCurrentTokenAsync(ct);
        var client = httpClientFactory.CreateClient("CjDropshipping");

        var url = $"v1/product/query?pid={Uri.EscapeDataString(pid)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("CJ-Access-Token", token);

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var envelope = JsonSerializer.Deserialize<CjApiResponse<CjProductDetailData>>(json, JsonOptions);

        if (envelope is null || !envelope.Result || envelope.Data is null)
        {
            logger.LogWarning(
                "CJDropshipping product detail returned unexpected response for pid={Pid}. Code={Code} Message={Message}",
                pid, envelope?.Code, envelope?.Message);
            return null;
        }

        var d = envelope.Data;

        decimal.TryParse(d.ProductWeight,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var weightGrams);

        decimal.TryParse(d.SellPrice,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var sellPrice);

        decimal.TryParse(d.SuggestSellPrice,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var suggestSellPrice);

        // Consolidate material from the set
        var material = d.MaterialNameEnSet is { Count: > 0 }
            ? string.Join(", ", d.MaterialNameEnSet.Where(m => !string.IsNullOrWhiteSpace(m)))
            : d.MaterialNameEn;

        var variants = (d.Variants ?? [])
            .Select(v => new CjProductDetailVariantDto(
                v.Vid,
                v.VariantNameEn,
                v.VariantImage,
                v.VariantSku,
                v.VariantKey,
                v.VariantSellPrice,
                v.VariantSugSellPrice,
                v.VariantWeight,
                v.VariantLength,
                v.VariantWidth,
                v.VariantHeight))
            .ToList();

        return new CjProductDetailDto(
            Pid: d.Pid,
            ProductNameEn: d.ProductNameEn,
            ProductSku: d.ProductSku,
            ImageUrls: d.ProductImageSet ?? [],
            Description: d.Description,
            WeightGrams: weightGrams > 0 ? weightGrams : null,
            Material: material,
            ProductKeyEn: d.ProductKeyEn,
            SellPrice: sellPrice > 0 ? sellPrice : null,
            SuggestSellPrice: suggestSellPrice > 0 ? suggestSellPrice : null,
            CategoryId: d.CategoryId,
            Variants: variants);
    }

    // ── Variant Stock ─────────────────────────────────────────────────────────

    public async Task<int?> GetVariantStockAsync(string vid, CancellationToken ct = default)
    {
        var token  = await tokenProvider.GetCurrentTokenAsync(ct);
        var client = httpClientFactory.CreateClient("CjDropshipping");

        var url = $"v1/product/stock/queryByVid?vid={Uri.EscapeDataString(vid)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("CJ-Access-Token", token);

        try
        {
            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var envelope = JsonSerializer.Deserialize<CjApiResponse<List<CjVariantStockItem>>>(json, JsonOptions);

            if (envelope is null || !envelope.Result || envelope.Data is null)
            {
                logger.LogWarning(
                    "CJ stock query returned unexpected response for vid={Vid}. Code={Code} Message={Message}",
                    vid, envelope?.Code, envelope?.Message);
                return null;
            }

            return envelope.Data.Sum(s => s.StorageNum);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "CJ stock query failed for vid={Vid}", vid);
            return null;
        }
    }

    // ── Product Reviews ────────────────────────────────────────────────────────────────────

    public async Task<CjReviewListDto> GetProductReviewsAsync(
        string pid, int page, int pageSize, CancellationToken ct = default)
    {
        var token  = await tokenProvider.GetCurrentTokenAsync(ct);
        var client = httpClientFactory.CreateClient("CjDropshipping");

        var url = $"v1/product/comments?pid={Uri.EscapeDataString(pid)}&pageNum={page}&pageSize={pageSize}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("CJ-Access-Token", token);

        try
        {
            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json     = await response.Content.ReadAsStringAsync(ct);
            var envelope = JsonSerializer.Deserialize<CjCommentsApiResponse<CjProductCommentsData>>(json, JsonOptions);

            bool isSuccess = envelope?.Code == 0 || envelope?.Success == true || envelope?.Result == true;
            if (!isSuccess || envelope?.Data is null)
            {
                logger.LogWarning(
                    "CJ reviews returned unexpected response for pid={Pid}. Code={Code} Message={Message}",
                    pid, envelope?.Code, envelope?.Message);
                return new CjReviewListDto(0, []);
            }

            var total = envelope.Data.Total ?? 0;

            var items = (envelope.Data.List ?? [])
                .Select(c => new CjReviewItemDto(
                        c.CommentId,
                        c.CommentUser,
                        c.Score ?? 0,
                        c.Comment ?? string.Empty,
                        c.CommentDate,
                        c.CountryCode,
                        c.FlagIconUrl,
                        c.CommentUrls ?? []))
                .ToList();

            return new CjReviewListDto(total, items);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "CJ reviews request failed for pid={Pid}", pid);
            return new CjReviewListDto(0, []);
        }
    }
}
