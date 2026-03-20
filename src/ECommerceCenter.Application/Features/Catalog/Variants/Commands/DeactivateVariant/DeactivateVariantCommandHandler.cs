using System.Text.Json;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.DeactivateVariant;

public class DeactivateVariantCommandHandler(
    IProductVariantRepository variantRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<DeactivateVariantCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await variantRepository
            .GetByIdAndProductAsync(request.VariantId, request.ProductId, cancellationToken);

        if (variant is null)
            return Result.NotFound("ProductVariant", request.VariantId);

        if (!variant.IsActive)
            return Result.Success("Variant is already inactive.");

        variant.IsActive  = false;
        variant.UpdatedAt = DateTime.UtcNow;

        variantRepository.Update(variant);
        await auditLogs.AddAsync(new AuditLog
        {
            ActorId    = currentUserService.UserId,
            ActorType  = ActorType.Admin,
            Action     = "Variant.Deactivate",
            EntityType = "ProductVariant",
            EntityId   = variant.Id,
            BeforeJson = JsonSerializer.Serialize(new { IsActive = true }),
            AfterJson  = JsonSerializer.Serialize(new { IsActive = false }),
            CreatedAt  = DateTime.UtcNow
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
