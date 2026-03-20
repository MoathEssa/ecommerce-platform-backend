using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Suppliers.Commands.ImportCjCategory;

public class ImportCjCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<ImportCjCategoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        ImportCjCategoryCommand request, CancellationToken cancellationToken)
    {
        // 1. Duplicate check on the root being imported ───────────────────────
        var existing = await categoryRepository.FindAllAsync(
            c => c.ExternalCategoryId == request.LeafId &&
                 c.Supplier == SupplierType.CjDropshipping,
            cancellationToken: cancellationToken);

        if (existing.Any())
            return Result<int>.Conflict(
                $"CJ category \"{request.LeafName}\" has already been imported into your store.");

        // 2. Build hierarchy + subtree atomically ─────────────────────────────
        int createdLeafId = 0;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            int? leafParentId = null;

            if (request.TargetLevel == 1)
            {
                // L1 root — no parent needed.
            }
            else if (request.TargetLevel == 2)
            {
                // L2: optionally find-or-create the L1 parent.
                if (request.IncludeChain && !string.IsNullOrWhiteSpace(request.Level1Name))
                {
                    var l1 = await FindOrCreateCategoryAsync(
                        request.Level1Id, request.Level1Name, null, request.MakeActive, ct);
                    leafParentId = l1.Id;
                }
            }
            else // TargetLevel == 3 (default)
            {
                if (request.IncludeChain)
                {
                    var l1 = await FindOrCreateCategoryAsync(
                        request.Level1Id, request.Level1Name, null, request.MakeActive, ct);
                    var l2 = await FindOrCreateCategoryAsync(
                        request.Level2Id, request.Level2Name, l1.Id, request.MakeActive, ct);
                    leafParentId = l2.Id;
                }
            }

            // ── Create the root category ──────────────────────────────────────
            var leaf = new Category
            {
                Name               = request.LeafName.Trim(),
                Slug               = await ResolveSlugAsync(request.LeafName, ct),
                IsActive           = request.MakeActive,
                SortOrder          = 0,
                ParentId           = leafParentId,
                ExternalCategoryId = request.LeafId,
                Supplier           = SupplierType.CjDropshipping,
                CreatedAt          = DateTime.UtcNow,
            };
            await categoryRepository.AddAsync(leaf, ct);
            await unitOfWork.SaveChangesAsync(ct);

            createdLeafId = leaf.Id;

            // ── Recursively import all children (L1 → L2+L3, L2 → L3) ────────
            if (request.SubTree is { Count: > 0 })
                await ImportSubtreeAsync(request.SubTree, leaf.Id, request.MakeActive, ct);

        }, cancellationToken);

        return Result<int>.Success(createdLeafId, "Category imported successfully.");
    }

    /// <summary>
    /// Finds an existing CJ category by external ID + parent, or creates it.
    /// Skips already-imported nodes so partial re-runs are safe.
    /// </summary>
    private async Task<Category> FindOrCreateCategoryAsync(
        string? externalId,
        string name,
        int? parentId,
        bool makeActive,
        CancellationToken ct)
    {
        var extId = externalId ?? name.Trim();

        var existing = (await categoryRepository.FindAllAsync(
            c => c.ExternalCategoryId == extId &&
                 c.Supplier == SupplierType.CjDropshipping &&
                 c.ParentId == parentId,
            cancellationToken: ct)).FirstOrDefault();

        if (existing is not null) return existing;

        var created = new Category
        {
            Name               = name.Trim(),
            Slug               = await ResolveSlugAsync(name, ct),
            IsActive           = makeActive,
            SortOrder          = 0,
            ParentId           = parentId,
            ExternalCategoryId = extId,
            Supplier           = SupplierType.CjDropshipping,
            CreatedAt          = DateTime.UtcNow,
        };
        await categoryRepository.AddAsync(created, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    /// <summary>
    /// Recursively find-or-creates every node in <paramref name="subtree"/> under
    /// <paramref name="parentId"/>, then recurses into each node's own children.
    /// Already-imported nodes are skipped, making the operation idempotent.
    /// </summary>
    private async Task ImportSubtreeAsync(
        List<ImportCjSubtreeItem> subtree,
        int parentId,
        bool makeActive,
        CancellationToken ct)
    {
        foreach (var item in subtree)
        {
            var node = await FindOrCreateCategoryAsync(
                item.Id, item.Name, parentId, makeActive, ct);

            if (item.Children.Count > 0)
                await ImportSubtreeAsync(item.Children, node.Id, makeActive, ct);
        }
    }

    /// <summary>
    /// Generates a slug for <paramref name="name"/>. If the base slug is already
    /// taken, draws one value from the shared <c>slug_suffix_seq</c> DB sequence
    /// and appends it — no loops, guaranteed unique.
    /// </summary>
    private async Task<string> ResolveSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = SlugHelper.Generate(name);
        if (!await categoryRepository.SlugExistsAsync(baseSlug, null, ct))
            return baseSlug;

        var suffix = await categoryRepository.NextSlugSuffixAsync(ct);
        return $"{baseSlug}-{suffix}";
    }
}
