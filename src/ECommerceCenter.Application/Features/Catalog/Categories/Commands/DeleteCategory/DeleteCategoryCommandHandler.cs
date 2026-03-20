using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;
using static ECommerceCenter.Application.Common.Errors.BusinessRuleCode;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(
        DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.NotFound("Category", request.Id);

        if (await categoryRepository.HasChildrenAsync(request.Id, cancellationToken))
            return Result.Failure(
                Error.BusinessRule(CategoryHasChildren,
                    "Cannot delete a category that has sub-categories. Remove or re-parent them first."),
                System.Net.HttpStatusCode.UnprocessableEntity);

        if (await categoryRepository.HasProductAssignmentsAsync(request.Id, cancellationToken))
            return Result.Failure(
                Error.BusinessRule(CategoryHasProducts,
                    "Cannot delete a category that has products assigned to it. Reassign them first."),
                System.Net.HttpStatusCode.UnprocessableEntity);

        var beforeJson = JsonSerializer.Serialize(new
        {
            category.Id, category.Name, category.Slug, category.IsActive
        });

        categoryRepository.Delete(category);
        await auditLogs.AddAsync(new AuditLog
        {
            ActorId    = currentUserService.UserId,
            ActorType  = ActorType.Admin,
            Action     = "Category.Delete",
            EntityType = "Category",
            EntityId   = category.Id,
            BeforeJson = beforeJson,
            CreatedAt  = DateTime.UtcNow
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
