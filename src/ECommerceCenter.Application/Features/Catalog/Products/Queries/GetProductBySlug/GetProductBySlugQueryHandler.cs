using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProductBySlug;

public class GetProductBySlugQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ICjProductService cjProductService)
    : IRequestHandler<GetProductBySlugQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(
        GetProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var row = await productRepository.GetProductDetailBySlugAsync(request.Slug, cancellationToken);

        if (row is null)
            return Result<ProductDetailDto>.NotFound("Product", request.Slug);

        var breadcrumbs = await BuildBreadcrumbsAsync(row.Category?.CategoryId ?? 0, cancellationToken);

        var categoryDto = row.Category is not null
            ? new ProductCategoryDto(row.Category.CategoryId, row.Category.Name, row.Category.Slug)
            : null;

        // Fetch CJ product detail for supplier-sourced products
        CjProductDetailDto? cjDetail = null;
        if (row.ExternalProductId is not null && row.Supplier == (int)SupplierType.CjDropshipping)
            cjDetail = await cjProductService.GetProductDetailAsync(row.ExternalProductId, cancellationToken);

        // Build variant DTOs — enrich with CJ variant names/images if available
        var variants = row.Variants.Select(v =>
        {
            var cjVariant = cjDetail?.Variants.FirstOrDefault(cv => cv.Vid == v.Sku || cv.VariantSku == v.Sku);
            return new ProductVariantDto(
                v.Id, v.Sku, OptionsJsonHelper.Parse(v.OptionsJson),
                v.BasePrice, v.CurrencyCode,
                StockThresholds.Map(v.OnHand),
                cjVariant?.VariantNameEn,
                cjVariant?.VariantImage);
        }).ToList();

        // Build image list — prefer CJ gallery when available
        var images = cjDetail?.ImageUrls is { Count: > 0 }
            ? cjDetail.ImageUrls.Select((url, i) => new ProductImageDto(0, url, null, i)).ToList()
            : row.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.VariantId, i.SortOrder)).ToList();

        var dto = new ProductDetailDto(
            row.Id, row.Title, row.Slug, row.Description,
            cjDetail?.Description, row.Brand, row.Status,
            cjDetail?.WeightGrams, cjDetail?.Material,
            breadcrumbs, categoryDto, variants, images, row.CreatedAt);

        return Result<ProductDetailDto>.Success(dto);
    }

    private async Task<List<BreadcrumbDto>> BuildBreadcrumbsAsync(int categoryId, CancellationToken ct)
    {
        if (categoryId == 0) return [];

        var all = await categoryRepository.GetAllActiveAsync(ct);
        var breadcrumbs = new List<BreadcrumbDto>();
        int? currentId = categoryId;

        while (currentId.HasValue)
        {
            var cat = all.FirstOrDefault(c => c.Id == currentId.Value);
            if (cat is null) break;
            breadcrumbs.Insert(0, new BreadcrumbDto(cat.Name, cat.Slug));
            currentId = cat.ParentId;
        }

        return breadcrumbs;
    }
}
