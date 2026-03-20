using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Commands.DeleteProductImage;

public record DeleteProductImageCommand(int ProductId, int ImageId) : IRequest<Result>;
