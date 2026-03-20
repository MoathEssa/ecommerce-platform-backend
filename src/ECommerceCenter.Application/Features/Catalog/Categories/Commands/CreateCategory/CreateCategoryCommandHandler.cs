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

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<CreateCategoryCommand, Result<AdminCategoryDto>>
{
    public async Task<Result<AdminCategoryDto>> Handle(
        CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var all = await categoryRepository.GetAllAsync(cancellationToken);
            var parentDepth = ComputeDepth(all, request.ParentId.Value);
            if (parentDepth >= 3)
                return Result<AdminCategoryDto>.Failure(
                    Error.BusinessRule(MaxCategoryDepthExceeded,
                        "Categories are limited to 3 levels deep."),
                    System.Net.HttpStatusCode.UnprocessableEntity);
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.Name)
            : request.Slug.Trim().ToLowerInvariant();

        if (await categoryRepository.SlugExistsAsync(slug, null, cancellationToken))
            return Result<AdminCategoryDto>.Duplicate("Slug", slug);

        if (await categoryRepository.SortOrderExistsAmongSiblingsAsync(
                request.ParentId, request.SortOrder, null, cancellationToken))
            return Result<AdminCategoryDto>.Failure(
                Error.BusinessRule(DuplicateSortOrderAmongSiblings,
                    $"A category with sort order {request.SortOrder} already exists under the same parent."),
                System.Net.HttpStatusCode.Conflict);

        var category = new Category
        {
            Name               = request.Name.Trim(),
            Slug               = slug,
            Description        = request.Description?.Trim(),
            ImageUrl           = request.ImageUrl?.Trim(),
            ParentId           = request.ParentId,
            SortOrder          = request.SortOrder,
            IsActive           = request.IsActive,
            ExternalCategoryId = request.ExternalCategoryId?.Trim(),
            Supplier           = request.Supplier.HasValue
                                    ? (ECommerceCenter.Domain.Enums.SupplierType)request.Supplier.Value
                                    : null,
            CreatedAt          = DateTime.UtcNow
        };

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await categoryRepository.AddAsync(category, ct);
            await unitOfWork.SaveChangesAsync(ct); // category.Id populated

            await auditLogs.AddAsync(new AuditLog
            {
                ActorId    = currentUserService.UserId,
                ActorType  = ActorType.Admin,
                Action     = "Category.Create",
                EntityType = "Category",
                EntityId   = category.Id,
                AfterJson  = JsonSerializer.Serialize(new
                {
                    category.Id, category.Name, category.Slug,
                    category.ParentId, category.IsActive
                }),
                CreatedAt = DateTime.UtcNow
            }, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return Result<AdminCategoryDto>.Success(new AdminCategoryDto(
            Id:                  category.Id,
            Name:                category.Name,
            Slug:                category.Slug,
            Description:         category.Description,
            ImageUrl:            category.ImageUrl,
            SortOrder:           category.SortOrder,
            IsActive:            category.IsActive,
            ParentId:            category.ParentId,
            CreatedAt:           category.CreatedAt,
            UpdatedAt:           category.UpdatedAt,
            ExternalCategoryId:  category.ExternalCategoryId,
            Supplier:            category.Supplier.HasValue ? (int)category.Supplier.Value : null),
            "Category created successfully");
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

}
