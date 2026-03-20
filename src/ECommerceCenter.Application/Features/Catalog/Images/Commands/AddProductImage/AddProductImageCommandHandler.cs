using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Commands.AddProductImage;

public class AddProductImageCommandHandler(
    IProductRepository productRepository,
    IProductImageRepository imageRepository,
    IImageStorageService imageStorage,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<AddProductImageCommand, Result<ProductImageDto>>
{
    public async Task<Result<ProductImageDto>> Handle(
        AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<ProductImageDto>.NotFound("Product", request.ProductId);

        var blobUrl = await imageStorage.UploadImageAsync(
            request.ImageContent, "products", request.FileName, request.ContentType,
            cancellationToken);

        var maxSort = await imageRepository.GetMaxSortOrderAsync(request.ProductId, cancellationToken);

        var image = new ProductImage
        {
            ProductId = request.ProductId,
            Url       = blobUrl,
            VariantId = request.VariantId,
            SortOrder = maxSort + 1,
            CreatedAt = DateTime.UtcNow
        };

        await imageRepository.AddAsync(image, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductImageDto>.Success(
            new ProductImageDto(image.Id, image.Url, image.VariantId, image.SortOrder));
    }
}
