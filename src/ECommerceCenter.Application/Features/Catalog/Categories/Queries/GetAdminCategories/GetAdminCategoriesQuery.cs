using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetAdminCategories;

public record GetAdminCategoriesQuery : IRequest<Result<List<AdminCategoryDto>>>;
