using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetAdminCategoryById;

public record GetAdminCategoryByIdQuery(int Id) : IRequest<Result<AdminCategoryDto>>;
