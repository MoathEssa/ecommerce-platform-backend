using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProductBySlug;

public record GetProductBySlugQuery(string Slug) : IRequest<Result<ProductDetailDto>>;
