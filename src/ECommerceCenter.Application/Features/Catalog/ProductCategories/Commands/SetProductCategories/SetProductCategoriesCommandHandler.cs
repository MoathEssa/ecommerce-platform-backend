using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.ProductCategories.Commands.SetProductCategories;

public class SetProductCategoriesCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<SetProductCategoriesCommand, Result>
{
    public async Task<Result> Handle(
        SetProductCategoriesCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound("Product", request.ProductId);

        // If assigning a category, validate it exists and is active
        if (request.CategoryId.HasValue)
        {
            var allActive = await categoryRepository.GetAllActiveAsync(cancellationToken);
            if (!allActive.Any(c => c.Id == request.CategoryId.Value))
                return Result.ValidationError(
                    $"Category ID {request.CategoryId.Value} was not found or is inactive.");
        }

        product.CategoryId = request.CategoryId;

        await auditLogs.AddAsync(new AuditLog
        {
            ActorId    = currentUserService.UserId,
            ActorType  = ActorType.Admin,
            Action     = "Product.SetCategory",
            EntityType = "Product",
            EntityId   = request.ProductId,
            AfterJson  = JsonSerializer.Serialize(new { categoryId = request.CategoryId }),
            CreatedAt  = DateTime.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
