using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<CreateProductCommand, Result<AdminProductCreatedDto>>
{
    public async Task<Result<AdminProductCreatedDto>> Handle(
        CreateProductCommand request, CancellationToken cancellationToken)
    {
        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.Title)
            : request.Slug.Trim().ToLowerInvariant();

        var slug = await ResolveUniqueSlugAsync(baseSlug, null, cancellationToken);

        var product = new Product
        {
            Title       = request.Title.Trim(),
            Slug        = slug,
            Description = request.Description?.Trim(),
            Brand       = request.Brand?.Trim(),
            Status      = ProductStatus.Draft,
            CreatedAt   = DateTime.UtcNow
        };

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await productRepository.AddAsync(product, ct);
            await unitOfWork.SaveChangesAsync(ct); // product.Id populated

            await auditLogs.AddAsync(new AuditLog
            {
                ActorId    = currentUserService.UserId,
                ActorType  = ActorType.Admin,
                Action     = "Product.Create",
                EntityType = "Product",
                EntityId   = product.Id,
                AfterJson  = JsonSerializer.Serialize(new
                {
                    product.Id, product.Title, product.Slug, Status = (int)product.Status
                }),
                CreatedAt = DateTime.UtcNow
            }, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return Result<AdminProductCreatedDto>.Success(new AdminProductCreatedDto(
            product.Id, product.Title, product.Slug, product.Description,
            product.Brand, (int)product.Status, product.CreatedAt));
    }

    // Single DB-sequence call — no polling loop, collision-safe under concurrent requests.
    private async Task<string> ResolveUniqueSlugAsync(
        string baseSlug, int? excludeId, CancellationToken ct)
    {
        if (!await productRepository.SlugExistsAsync(baseSlug, excludeId, ct))
            return baseSlug;

        var suffix = await productRepository.NextSlugSuffixAsync(ct);
        return $"{baseSlug}-{suffix}";
    }
}
