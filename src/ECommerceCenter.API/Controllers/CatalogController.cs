using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Features.Catalog.Categories.Commands.CreateCategory;
using ECommerceCenter.Application.Features.Catalog.Categories.Commands.DeleteCategory;
using ECommerceCenter.Application.Features.Catalog.Categories.Commands.UpdateCategory;
using ECommerceCenter.Application.Features.Catalog.Categories.Commands.UploadCategoryImage;
using ECommerceCenter.Application.Features.Catalog.Images.Commands.AddProductImage;
using ECommerceCenter.Application.Features.Catalog.Images.Commands.DeleteProductImage;
using ECommerceCenter.Application.Features.Catalog.Images.Commands.ReorderProductImages;
using ECommerceCenter.Application.Features.Catalog.Images.Queries.GetProductImages;
using ECommerceCenter.Application.Features.Catalog.ProductCategories.Commands.SetProductCategories;
using ECommerceCenter.Application.Features.Catalog.Products.Commands.BulkChangeProductStatus;
using ECommerceCenter.Application.Features.Catalog.Products.Commands.ChangeProductStatus;
using ECommerceCenter.Application.Features.Catalog.Products.Commands.CreateProduct;
using ECommerceCenter.Application.Features.Catalog.Products.Commands.UpdateProduct;
using ECommerceCenter.Application.Features.Catalog.Products.Queries.GetAdminProductDetail;
using ECommerceCenter.Application.Features.Catalog.Products.Queries.GetAdminProducts;
using ECommerceCenter.Application.Features.Catalog.Variants.Commands.AddVariant;
using ECommerceCenter.Application.Features.Catalog.Variants.Commands.DeactivateVariant;
using ECommerceCenter.Application.Features.Catalog.Variants.Commands.UpdateVariant;
using ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetAdminCategories;
using ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetAdminCategoryById;
using ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetCategoryBySlug;
using ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetCategoryTree;
using ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProductBySlug;
using ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProducts;
using ECommerceCenter.Application.Features.Catalog.Products.Queries.GetSearchSuggestions;
using ECommerceCenter.Application.Features.Catalog.Suppliers.Queries.GetCjCategories;
using ECommerceCenter.Application.Features.Catalog.Suppliers.Queries.GetCjProducts;
using ECommerceCenter.Application.Features.Catalog.Suppliers.Queries.GetCjProductVariants;
using ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.BulkImportCjProducts;
using ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.ImportCjProduct;
using ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.ImportCjCategory;
using ECommerceCenter.Application.Features.Catalog.Variants.Queries.GetVariantDetail;
using ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProductReviews;
using MediatR;
using ECommerceCenter.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[Route("api/v1/catalog")]
public class CatalogController(IMediator mediator) : AppController(mediator)
{
    // ── Storefront ─────────────────────────────────────────────────────────



    [AllowAnonymous]
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] bool flat = false,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetCategoryTreeQuery(flat), ct));


    [AllowAnonymous]
    [HttpGet("categories/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetCategoryBySlugQuery(slug), ct));

    [AllowAnonymous]
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? categorySlug = null,
        [FromQuery] string? search = null,
        [FromQuery] string? brand = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string sortBy = "relevance",
        [FromQuery] bool? inStock = null,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new GetProductsQuery(page, pageSize, categorySlug, search, brand,
                minPrice, maxPrice, sortBy, inStock), ct));

    [AllowAnonymous]
    [HttpGet("products/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetProductBySlugQuery(slug), ct));

    [AllowAnonymous]
    [HttpGet("products/{slug}/variants/{variantId:int}")]
    public async Task<IActionResult> GetVariantDetail(
        string slug, int variantId, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetVariantDetailQuery(slug, variantId), ct));

    [AllowAnonymous]
    [HttpGet("products/{slug}/reviews")]
    public async Task<IActionResult> GetProductReviews(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetProductReviewsQuery(slug, page, pageSize), ct));

    [AllowAnonymous]
    [HttpGet("search/suggestions")]
    public async Task<IActionResult> GetSearchSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 8,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetSearchSuggestionsQuery(q, limit), ct));

    // ── Products (admin) ───────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/products")]
    public async Task<IActionResult> AdminGetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "newest",
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new GetAdminProductsQuery(page, pageSize, status, search, sortBy), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/products/{id:int}")]
    public async Task<IActionResult> AdminGetProductDetail(int id, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetAdminProductDetailQuery(id), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/products")]
    public async Task<IActionResult> AdminCreateProduct(
        [FromBody] CreateProductCommand command, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(command, ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("admin/products/{id:int}")]
    public async Task<IActionResult> AdminUpdateProduct(
        int id, [FromBody] UpdateProductBody body, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new UpdateProductCommand(id, body.Title, body.Slug, body.Description, body.Brand), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("admin/products/{id:int}/status")]
    public async Task<IActionResult> AdminChangeProductStatus(
        int id, [FromBody] ChangeProductStatusBody body, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new ChangeProductStatusCommand(id, body.Status), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("admin/products/bulk-status")]
    public async Task<IActionResult> AdminBulkChangeProductStatus(
        [FromBody] BulkChangeProductStatusBody body, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new BulkChangeProductStatusCommand(body.Ids, body.Status), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("admin/products/{productId:int}/categories")]
    public async Task<IActionResult> AdminSetProductCategory(
        int productId, [FromBody] SetProductCategoryBody body, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new SetProductCategoriesCommand(productId, body.CategoryId), ct));

    // ── Variants (admin) ───────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/products/{productId:int}/variants")]
    public async Task<IActionResult> AdminAddVariant(
        int productId, [FromBody] AddVariantBody body, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new AddVariantCommand(productId, body.Options, body.BasePrice,
                body.CurrencyCode, body.IsActive, body.InitialStock), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("admin/products/{productId:int}/variants/{variantId:int}")]
    public async Task<IActionResult> AdminUpdateVariant(
        int productId, int variantId, [FromBody] UpdateVariantBody body, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new UpdateVariantCommand(productId, variantId, body.Sku, body.Options,
                body.BasePrice, body.CurrencyCode, body.IsActive), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("admin/products/{productId:int}/variants/{variantId:int}")]
    public async Task<IActionResult> AdminDeactivateVariant(
        int productId, int variantId, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new DeactivateVariantCommand(productId, variantId), ct));

    // ── Images (admin) ─────────────────────────────────────────────────────

    /// <summary>Returns all images for a product, ordered by SortOrder.</summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/products/{productId:int}/images")]
    public async Task<IActionResult> AdminGetProductImages(
        int productId, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetProductImagesQuery(productId), ct));

    /// <summary>
    /// Uploads an image file directly to Azure Blob Storage (server-side SAS)
    /// and adds it to the product's image gallery.
    /// Accepts multipart/form-data with a single "image" file field.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/products/{productId:int}/images")]
    public async Task<IActionResult> AdminAddImage(
        int productId,
        IFormFile image,
        [FromForm] int? variantId = null,
        CancellationToken ct = default)
    {
        if (image is null || image.Length == 0)
            return BadRequest("No image file provided.");

        return HandleResult(await Mediator.Send(
            new AddProductImageCommand(
                productId,
                image.OpenReadStream(),
                image.FileName,
                image.ContentType,
                variantId),
            ct));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("admin/products/{productId:int}/images/order")]
    public async Task<IActionResult> AdminReorderImages(
        int productId, [FromBody] List<int> imageIds, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new ReorderProductImagesCommand(productId, imageIds), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("admin/products/{productId:int}/images/{imageId:int}")]
    public async Task<IActionResult> AdminDeleteImage(
        int productId, int imageId, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new DeleteProductImageCommand(productId, imageId), ct));

    // ── Categories (admin) ─────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/categories")]
    public async Task<IActionResult> AdminGetCategories(CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetAdminCategoriesQuery(), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/categories/{id:int}")]
    public async Task<IActionResult> AdminGetCategoryById(int id, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetAdminCategoryByIdQuery(id), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/categories")]
    public async Task<IActionResult> AdminCreateCategory(
        [FromBody] CreateCategoryCommand command, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(command, ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("admin/categories/{id:int}")]
    public async Task<IActionResult> AdminUpdateCategory(
        int id, [FromBody] UpdateCategoryBody body, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new UpdateCategoryCommand(id, body.Name, body.Slug, body.Description,
                body.ImageUrl, body.ParentId, body.SortOrder, body.IsActive), ct));

    /// <summary>
    /// Uploads a new image for a category directly to Azure Blob Storage (server-side SAS)
    /// and updates the category's ImageUrl.
    /// Accepts multipart/form-data with a single "image" file field.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("admin/categories/{id:int}/image")]
    public async Task<IActionResult> AdminUploadCategoryImage(
        int id,
        IFormFile image,
        CancellationToken ct = default)
    {
        if (image is null || image.Length == 0)
            return BadRequest("No image file provided.");

        return HandleResult(await Mediator.Send(
            new UploadCategoryImageCommand(id, image.OpenReadStream(), image.FileName, image.ContentType),
            ct));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("admin/categories/{id:int}")]
    public async Task<IActionResult> AdminDeleteCategory(int id, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new DeleteCategoryCommand(id), ct));

    // ── Supplier Catalog (admin) ───────────────────────────────────────────

    /// <summary>
    /// Returns the full CJDropshipping category tree (3 levels).
    /// Only Level-3 nodes carry a <c>categoryId</c> which must be stored as
    /// <c>externalCategoryId</c> when importing a category.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/suppliers/cj/categories")]
    public async Task<IActionResult> AdminGetCjCategories(CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetCjCategoriesQuery(), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/suppliers/cj/products")]
    public async Task<IActionResult> AdminGetCjProducts(
        [FromQuery] string? keyWord,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? categoryId = null,
        [FromQuery] string? countryCode = null,
        [FromQuery] decimal? startSellPrice = null,
        [FromQuery] decimal? endSellPrice = null,
        [FromQuery] int? addMarkStatus = null,
        [FromQuery] int? productType = null,
        [FromQuery] string? sort = null,
        [FromQuery] int? orderBy = null,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetCjProductsQuery(
            keyWord, page, size, categoryId, countryCode,
            startSellPrice, endSellPrice, addMarkStatus, productType,
            sort, orderBy), ct));

    /// <summary>
    /// Imports a CJDropshipping category (and optionally its L1/L2 parent chain)
    /// atomically. Reuses existing parents; returns 409 if the leaf is already imported.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/suppliers/cj/categories/import")]
    public async Task<IActionResult> AdminImportCjCategory(
        [FromBody] ImportCjCategoryCommand command, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(command, ct));

    /// <summary>
    /// Imports a CJDropshipping product into the local store catalogue.
    /// Returns 422 if the product's CJ category has not been imported yet.
    /// Returns 409 if the product was already imported.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/suppliers/cj/products/import")]
    public async Task<IActionResult> AdminImportCjProduct(
        [FromBody] ImportCjProductCommand command, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(command, ct));

    /// <summary>
    /// Bulk-imports multiple CJDropshipping variants in a single transaction.
    /// Skips already-imported variants; returns counts of imported/skipped/errors.
    /// Returns 422 if the CJ category has not been imported yet.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/suppliers/cj/products/bulk-import")]
    public async Task<IActionResult> AdminBulkImportCjProducts(
        [FromBody] BulkImportCjProductsCommand command, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(command, ct));

    /// <summary>
    /// Returns all CJDropshipping variants for a given product ID.
    /// Optionally filtered by country code to restrict results to stocked variants.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/suppliers/cj/products/{pid}/variants")]
    public async Task<IActionResult> AdminGetCjProductVariants(
        string pid,
        [FromQuery] string? countryCode = null,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetCjProductVariantsQuery(pid, countryCode), ct));
}
