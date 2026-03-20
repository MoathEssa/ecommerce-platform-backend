using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.UploadCategoryImage;

public class UploadCategoryImageCommandHandler(
    ICategoryRepository categoryRepository,
    IImageStorageService imageStorage,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<UploadCategoryImageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        UploadCategoryImageCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<string>.NotFound("Category", request.CategoryId);

        var blobUrl = await imageStorage.UploadImageAsync(
            request.ImageContent, "categories", request.FileName, request.ContentType,
            cancellationToken);

        // Delete old image blob if one exists
        if (!string.IsNullOrWhiteSpace(category.ImageUrl))
            await imageStorage.DeleteAsync(category.ImageUrl, cancellationToken);

        category.ImageUrl = blobUrl;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(blobUrl);
    }
}
