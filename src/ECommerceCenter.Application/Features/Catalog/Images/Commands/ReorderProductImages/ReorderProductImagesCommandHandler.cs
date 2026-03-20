using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Commands.ReorderProductImages;

public class ReorderProductImagesCommandHandler(
    IProductImageRepository imageRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<ReorderProductImagesCommand, Result>
{
    public async Task<Result> Handle(
        ReorderProductImagesCommand request, CancellationToken cancellationToken)
    {
        var existing = await imageRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (!existing.Any())
            return Result.NotFound("Product images", request.ProductId);

        var existingIds = existing.Select(i => i.Id).ToHashSet();
        if (request.ImageIds.Any(id => !existingIds.Contains(id)))
            return Result.ValidationError("One or more image IDs do not belong to this product.");

        var imageMap = existing.ToDictionary(i => i.Id);
        for (var idx = 0; idx < request.ImageIds.Count; idx++)
        {
            if (!imageMap.TryGetValue(request.ImageIds[idx], out var image))
                continue;
            image.SortOrder = idx;
            imageRepository.Update(image);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
