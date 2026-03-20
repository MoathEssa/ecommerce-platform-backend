using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Queries.GetCjCategories;

public record GetCjCategoriesQuery : IRequest<Result<List<CjCategoryNodeDto>>>;

public class GetCjCategoriesQueryHandler(
    ICjProductService cjProductService,
    ICategoryRepository categoryRepository)
    : IRequestHandler<GetCjCategoriesQuery, Result<List<CjCategoryNodeDto>>>
{
    public async Task<Result<List<CjCategoryNodeDto>>> Handle(
        GetCjCategoriesQuery request, CancellationToken cancellationToken)
    {
        var tree = await cjProductService.GetCategoriesAsync(cancellationToken);

        // Load all previously-imported CJ category external IDs in one DB round-trip.
        var imported = await categoryRepository.FindAllAsync(
            c => c.Supplier == SupplierType.CjDropshipping,
            cancellationToken: cancellationToken);

        var importedIds = imported
            .Where(c => c.ExternalCategoryId != null)
            .Select(c => c.ExternalCategoryId!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Result<List<CjCategoryNodeDto>>.Success(StampImported(tree, importedIds));
    }

    /// <summary>Recursively marks each node whose <c>CategoryId</c> is already in the store.</summary>
    private static List<CjCategoryNodeDto> StampImported(
        List<CjCategoryNodeDto> nodes, HashSet<string> importedIds) =>
        nodes.Select(n => n with
        {
            IsImported = n.CategoryId != null && importedIds.Contains(n.CategoryId),
            Children   = StampImported(n.Children, importedIds),
        }).ToList();
}
