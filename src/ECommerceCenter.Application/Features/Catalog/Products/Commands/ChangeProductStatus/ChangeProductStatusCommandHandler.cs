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

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.ChangeProductStatus;

public class ChangeProductStatusCommandHandler(
    IProductRepository productRepository,
    IProductVariantRepository variantRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<ChangeProductStatusCommand, Result>
{
    // Allowed transitions: Draft→Active, Active→Archived, Archived→Active, Archived→Draft
    private static readonly Dictionary<ProductStatus, HashSet<ProductStatus>> AllowedTransitions = new()
    {
        [ProductStatus.Draft]    = [ProductStatus.Active],
        [ProductStatus.Active]   = [ProductStatus.Archived],
        [ProductStatus.Archived] = [ProductStatus.Active, ProductStatus.Draft]
    };

    public async Task<Result> Handle(
        ChangeProductStatusCommand request, CancellationToken cancellationToken)
    {
        var newStatus = (ProductStatus)request.Status;

        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.NotFound("Product", request.Id);


        if (!AllowedTransitions.TryGetValue(product.Status, out var allowed) ||
            !allowed.Contains(newStatus))
            return Result.Failure(
                Error.BusinessRule(InvalidStatusTransition,
                    $"Cannot transition from '{product.Status}' to '{newStatus}'."),
                System.Net.HttpStatusCode.UnprocessableEntity);

        // Activating requires at least one active variant
        if (newStatus == ProductStatus.Active)
        {
            var hasActive = await variantRepository.HasActiveVariantsAsync(request.Id, cancellationToken);
            if (!hasActive)
                return Result.Failure(
                    Error.BusinessRule(NoActiveVariants,
                        "A product must have at least one active variant before it can be activated."),
                    System.Net.HttpStatusCode.UnprocessableEntity);
        }

        var beforeStatus = (int)product.Status;
        product.Status    = newStatus;
        product.UpdatedAt = DateTime.UtcNow;

        productRepository.Update(product);
        await auditLogs.AddAsync(new AuditLog
        {
            ActorId    = currentUserService.UserId,
            ActorType  = ActorType.Admin,
            Action     = "Product.StatusChange",
            EntityType = "Product",
            EntityId   = product.Id,
            BeforeJson = JsonSerializer.Serialize(new { Status = beforeStatus }),
            AfterJson  = JsonSerializer.Serialize(new { Status = (int)newStatus }),
            CreatedAt  = DateTime.UtcNow
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
