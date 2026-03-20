using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.BulkChangeProductStatus;

public class BulkChangeProductStatusCommandHandler(
    IProductRepository productRepository,
    IProductVariantRepository variantRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<BulkChangeProductStatusCommand, Result<int>>
{
    private static readonly Dictionary<ProductStatus, HashSet<ProductStatus>> AllowedTransitions = new()
    {
        [ProductStatus.Draft]    = [ProductStatus.Active],
        [ProductStatus.Active]   = [ProductStatus.Archived],
        [ProductStatus.Archived] = [ProductStatus.Active, ProductStatus.Draft]
    };

    public async Task<Result<int>> Handle(
        BulkChangeProductStatusCommand request, CancellationToken cancellationToken)
    {
        var newStatus = (ProductStatus)request.Status;

        var products = (await productRepository.FindAllAsync(
            p => request.Ids.Contains(p.Id),
            cancellationToken: cancellationToken)).ToList();

        var updatedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var product in products)
        {
            if (!AllowedTransitions.TryGetValue(product.Status, out var allowed) ||
                !allowed.Contains(newStatus))
                continue;

            if (newStatus == ProductStatus.Active)
            {
                var hasActive = await variantRepository.HasActiveVariantsAsync(product.Id, cancellationToken);
                if (!hasActive) continue;
            }

            var beforeStatus = (int)product.Status;
            product.Status    = newStatus;
            product.UpdatedAt = now;

            productRepository.Update(product);
            await auditLogs.AddAsync(new AuditLog
            {
                ActorId    = currentUserService.UserId,
                ActorType  = ActorType.Admin,
                Action     = "Product.BulkStatusChange",
                EntityType = "Product",
                EntityId   = product.Id,
                BeforeJson = JsonSerializer.Serialize(new { Status = beforeStatus }),
                AfterJson  = JsonSerializer.Serialize(new { Status = (int)newStatus }),
                CreatedAt  = now
            }, cancellationToken);

            updatedCount++;
        }

        if (updatedCount > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(updatedCount);
    }
}
