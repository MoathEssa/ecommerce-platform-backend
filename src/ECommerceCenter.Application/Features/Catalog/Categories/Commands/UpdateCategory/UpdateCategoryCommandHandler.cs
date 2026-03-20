using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<UpdateCategoryCommand, Result<AdminCategoryDto>>
{
    public async Task<Result<AdminCategoryDto>> Handle(
        UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result<AdminCategoryDto>.NotFound("Category", request.Id);

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                return Result<AdminCategoryDto>.ValidationError("A category cannot be its own parent.");

            var all = await categoryRepository.GetAllAsync(cancellationToken);

            if (IsDescendant(all, request.Id, request.ParentId.Value))
                return Result<AdminCategoryDto>.ValidationError(
                    "Cannot move a category under one of its own descendants.");

            var parentDepth   = ComputeDepth(all, request.ParentId.Value);
            var subtreeHeight = GetSubtreeHeight(all, request.Id);
            if (parentDepth + subtreeHeight > 3)
                return Result<AdminCategoryDto>.Failure(
                    Error.BusinessRule(MaxCategoryDepthExceeded,
                        "This move would exceed the maximum category depth of 3."),
                    System.Net.HttpStatusCode.UnprocessableEntity);
        }

        var beforeJson = JsonSerializer.Serialize(new
        {
            category.Name, category.Slug, category.ParentId, category.IsActive
        });

        var targetSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.Name)
            : request.Slug.Trim().ToLowerInvariant();

        if (targetSlug != category.Slug &&
            await categoryRepository.SlugExistsAsync(targetSlug, request.Id, cancellationToken))
            return Result<AdminCategoryDto>.Duplicate("Slug", targetSlug);

        if (category.SortOrder != request.SortOrder &&
            await categoryRepository.SortOrderExistsAmongSiblingsAsync(
                request.ParentId, request.SortOrder, request.Id, cancellationToken))
            return Result<AdminCategoryDto>.Failure(
                Error.BusinessRule(DuplicateSortOrderAmongSiblings,
                    $"A category with sort order {request.SortOrder} already exists under the same parent."),
                System.Net.HttpStatusCode.Conflict);

        category.Name        = request.Name.Trim();
        category.Slug        = targetSlug;
        category.Description = request.Description?.Trim();
        category.ImageUrl    = request.ImageUrl?.Trim();
        category.ParentId    = request.ParentId;
        category.SortOrder   = request.SortOrder;
        category.IsActive    = request.IsActive;
        category.UpdatedAt   = DateTime.UtcNow;
        // Note: ExternalCategoryId and Supplier are not changed on update
        // to preserve the supplier linkage once a category is imported.

        categoryRepository.Update(category);

        await auditLogs.AddAsync(new AuditLog
        {
            ActorId    = currentUserService.UserId,
            ActorType  = ActorType.Admin,
            Action     = "Category.Update",
            EntityType = "Category",
            EntityId   = category.Id,
            BeforeJson = beforeJson,
            AfterJson  = JsonSerializer.Serialize(new
            {
                category.Name, category.Slug, category.ParentId, category.IsActive
            }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminCategoryDto>.Success(null!,"Category updated successfully.");
    }

    private static int ComputeDepth(List<Category> all, int categoryId)
    {
        var depth = 0;
        int? current = categoryId;
        while (current.HasValue)
        {
            depth++;
            current = all.FirstOrDefault(c => c.Id == current.Value)?.ParentId;
        }
        return depth;
    }

    private static int GetSubtreeHeight(List<Category> all, int rootId)
    {
        var childLookup = all.ToLookup(c => c.ParentId, c => c.Id);
        var maxHeight = 1;
        var stack = new Stack<(int Id, int Depth)>();
        stack.Push((rootId, 1));
        while (stack.Count > 0)
        {
            var (id, depth) = stack.Pop();
            if (depth > maxHeight) maxHeight = depth;
            foreach (var childId in childLookup[id])
                stack.Push((childId, depth + 1));
        }
        return maxHeight;
    }

    private static bool IsDescendant(List<Category> all, int ancestorId, int candidateId)
    {
        int? current = all.FirstOrDefault(c => c.Id == candidateId)?.ParentId;
        while (current.HasValue)
        {
            if (current.Value == ancestorId) return true;
            current = all.FirstOrDefault(c => c.Id == current.Value)?.ParentId;
        }
        return false;
    }
}
