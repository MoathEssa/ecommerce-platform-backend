using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetAdminProductDetail;

public record GetAdminProductDetailQuery(int Id) : IRequest<Result<AdminProductDetailDto>>;
