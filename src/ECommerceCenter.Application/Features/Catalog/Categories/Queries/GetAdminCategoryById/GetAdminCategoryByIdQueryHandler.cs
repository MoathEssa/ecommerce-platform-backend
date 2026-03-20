using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetAdminCategoryById;

public class GetAdminCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetAdminCategoryByIdQuery, Result<AdminCategoryDto>>
{
    public async Task<Result<AdminCategoryDto>> Handle(
        GetAdminCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return Result<AdminCategoryDto>.NotFound("Category", request.Id.ToString());

        return Result<AdminCategoryDto>.Success(new AdminCategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.SortOrder,
            category.IsActive,
            category.ParentId,
            category.CreatedAt,
            category.UpdatedAt,
            ExternalCategoryId: category.ExternalCategoryId,
            Supplier:           category.Supplier.HasValue ? (int)category.Supplier.Value : null));
    }
}
