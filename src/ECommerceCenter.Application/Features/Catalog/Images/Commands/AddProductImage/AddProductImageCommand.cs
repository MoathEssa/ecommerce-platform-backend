using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Commands.AddProductImage;

/// <summary>
/// Uploads an image stream to Azure Blob Storage and adds it to the product.
/// The stream is uploaded by the backend — the SAS URL is never exposed to clients.
/// </summary>
public record AddProductImageCommand(
    int ProductId,
    Stream ImageContent,
    string FileName,
    string ContentType,
    int? VariantId) : IRequest<Result<ProductImageDto>>;
