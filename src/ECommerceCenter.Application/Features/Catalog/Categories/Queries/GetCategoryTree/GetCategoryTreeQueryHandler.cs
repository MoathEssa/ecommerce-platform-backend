using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetCategoryTree;

public class GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoryTreeQuery, Result<List<CategoryTreeDto>>>
{
    public async Task<Result<List<CategoryTreeDto>>> Handle(
        GetCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllActiveAsync(cancellationToken);

        if (request.Flat)
        {
            var flat = categories
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryTreeDto(c.Id, c.Name, c.Slug, c.ImageUrl, c.SortOrder, []))
                .ToList();

            return Result<List<CategoryTreeDto>>.Success(flat);
        }

        var childLookup = categories.ToLookup(c => c.ParentId);

        List<CategoryTreeDto> BuildChildren(int? parentId) =>
            childLookup[parentId]
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryTreeDto(c.Id, c.Name, c.Slug, c.ImageUrl, c.SortOrder, BuildChildren(c.Id)))
                .ToList();

        return Result<List<CategoryTreeDto>>.Success(BuildChildren(null));
    }
}
