using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Images.Queries.GetProductImages;

public record GetProductImagesQuery(int ProductId) : IRequest<Result<List<ProductImageDto>>>;
