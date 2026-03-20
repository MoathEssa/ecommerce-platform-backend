using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.UploadCategoryImage;

/// <summary>
/// Streams the image to Azure Blob Storage and updates the category's ImageUrl.
/// The SAS URL is generated and consumed entirely on the backend.
/// </summary>
public record UploadCategoryImageCommand(
    int CategoryId,
    Stream ImageContent,
    string FileName,
    string ContentType) : IRequest<Result<string>>;
