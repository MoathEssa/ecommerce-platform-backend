using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetCategoryBySlug;

public record GetCategoryBySlugQuery(string Slug) : IRequest<Result<CategoryDetailDto>>;
