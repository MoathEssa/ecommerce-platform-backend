using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Commands.DeleteProductImage;

public class DeleteProductImageCommandHandler(
    IProductImageRepository imageRepository,
    IImageStorageService imageStorage,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductImageCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await imageRepository
            .GetByIdAndProductAsync(request.ImageId, request.ProductId, cancellationToken);

        if (image is null)
            return Result.NotFound("ProductImage", request.ImageId);

        // Capture the URL before deleting the row so we can clean up blob storage afterward
        var blobUrl = image.Url;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            imageRepository.Delete(image);
            await unitOfWork.SaveChangesAsync(ct);

            // Re-index remaining images to keep SortOrder contiguous
            var remaining = await imageRepository.GetByProductIdAsync(request.ProductId, ct);
            for (var idx = 0; idx < remaining.Count; idx++)
            {
                remaining[idx].SortOrder = idx;
                imageRepository.Update(remaining[idx]);
            }

            if (remaining.Count > 0)
                await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        // Delete the blob after the DB transaction commits (best-effort; storage failure won't
        // roll back the DB change — orphaned blobs can be cleaned up by a background job)
        await imageStorage.DeleteAsync(blobUrl, cancellationToken);

        return Result.Success();
    }
}
