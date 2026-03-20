using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetAdminProductDetail;

public class GetAdminProductDetailQueryHandler(
    IProductRepository productRepository,
    ICjProductService cjProductService)
    : IRequestHandler<GetAdminProductDetailQuery, Result<AdminProductDetailDto>>
{
    public async Task<Result<AdminProductDetailDto>> Handle(
        GetAdminProductDetailQuery request, CancellationToken cancellationToken)
    {
        var row = await productRepository.GetAdminProductDetailByIdAsync(request.Id, cancellationToken);

        if (row is null)
            return Result<AdminProductDetailDto>.NotFound("Product", request.Id);

        // Fetch CJ product detail for supplier-sourced products
        CjProductDetailDto? cjDetail = null;
        if (row.ExternalProductId is not null && row.Supplier == (int)SupplierType.CjDropshipping)
            cjDetail = await cjProductService.GetProductDetailAsync(row.ExternalProductId, cancellationToken);

        var images = cjDetail?.ImageUrls is { Count: > 0 }
            ? cjDetail.ImageUrls.Select((url, i) => new ProductImageDto(0, url, null, i)).ToList()
            : row.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.VariantId, i.SortOrder)).ToList();

        var dto = new AdminProductDetailDto(
            row.Id, row.Title, row.Slug, row.Description,
            cjDetail?.Description, row.Brand, row.Status,
            cjDetail?.WeightGrams, cjDetail?.Material,
            row.Category is not null
                ? new ProductCategoryDto(row.Category.CategoryId, row.Category.Name, row.Category.Slug)
                : null,
            row.Variants.Select(v => new AdminProductVariantDto(
                v.Id, v.Sku, OptionsJsonHelper.Parse(v.OptionsJson),
                v.BasePrice, v.SupplierPrice, v.CurrencyCode, v.IsActive,
                new AdminInventoryDto(v.OnHand, v.OnHand))).ToList(),
            images,
            row.CreatedAt, row.UpdatedAt);

        return Result<AdminProductDetailDto>.Success(dto);
    }
}
