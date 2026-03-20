using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetCategoryBySlug;

public class GetCategoryBySlugQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoryBySlugQuery, Result<CategoryDetailDto>>
{
    public async Task<Result<CategoryDetailDto>> Handle(
        GetCategoryBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var all = await categoryRepository.GetAllActiveAsync(cancellationToken);

        var category = all.FirstOrDefault(c => c.Slug == request.Slug);

        if (category is null)
            return Result<CategoryDetailDto>.NotFound("Category", request.Slug);

        var breadcrumbs = new List<BreadcrumbDto>();
        var current = category;
        while (current is not null)
        {
            breadcrumbs.Insert(0, new BreadcrumbDto(current.Name, current.Slug));
            current = current.ParentId.HasValue
                ? all.FirstOrDefault(c => c.Id == current.ParentId.Value)
                : null;
        }

        var children = all
            .Where(c => c.ParentId == category.Id)
            .Select(c => new CategoryChildDto(c.Id, c.Name, c.Slug, c.ImageUrl))
            .ToList();

        return Result<CategoryDetailDto>.Success(new CategoryDetailDto(
            category.Id, category.Name, category.Slug, category.Description,
            category.ImageUrl, breadcrumbs, children));
    }
}
