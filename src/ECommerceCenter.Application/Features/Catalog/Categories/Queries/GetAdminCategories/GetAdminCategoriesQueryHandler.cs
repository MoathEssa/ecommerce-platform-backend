using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetAdminCategories;

public class GetAdminCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetAdminCategoriesQuery, Result<List<AdminCategoryDto>>>
{
    public async Task<Result<List<AdminCategoryDto>>> Handle(
        GetAdminCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);

        var dtos = categories
            .OrderBy(c => c.SortOrder)
            .Select(c => new AdminCategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.ImageUrl,
                c.SortOrder,
                c.IsActive,
                c.ParentId,
                c.CreatedAt,
                c.UpdatedAt,
                ExternalCategoryId: c.ExternalCategoryId,
                Supplier: c.Supplier.HasValue ? (int)c.Supplier.Value : null))
            .ToList();

        return Result<List<AdminCategoryDto>>.Success(dtos);
    }
}
