using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Commands.ReorderProductImages;

public record ReorderProductImagesCommand(
    int ProductId,
    List<int> ImageIds) : IRequest<Result>;
