using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IEfUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IGenericRepository<AuditLog> auditLogs)
    : IRequestHandler<UpdateProductCommand, Result<AdminProductCreatedDto>>
{
    public async Task<Result<AdminProductCreatedDto>> Handle(
        UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result<AdminProductCreatedDto>.NotFound("Product", request.Id);

        var beforeJson = JsonSerializer.Serialize(new
        {
            product.Id, product.Title, product.Slug, Status = (int)product.Status
        });

        var targetSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.Title)
            : request.Slug.Trim().ToLowerInvariant();

        if (targetSlug != product.Slug)
        {
            // Single sequence call — no polling loop
            if (await productRepository.SlugExistsAsync(targetSlug, product.Id, cancellationToken))
            {
                var suffix = await productRepository.NextSlugSuffixAsync(cancellationToken);
                targetSlug = $"{targetSlug}-{suffix}";
            }
            product.Slug = targetSlug;
        }

        product.Title       = request.Title.Trim();
        product.Description = request.Description?.Trim();
        product.Brand       = request.Brand?.Trim();
        product.UpdatedAt   = DateTime.UtcNow;

        productRepository.Update(product);

        await auditLogs.AddAsync(new AuditLog
        {
            ActorId    = currentUserService.UserId,
            ActorType  = ActorType.Admin,
            Action     = "Product.Update",
            EntityType = "Product",
            EntityId   = product.Id,
            BeforeJson = beforeJson,
            AfterJson  = JsonSerializer.Serialize(new
            {
                product.Id, product.Title, product.Slug, Status = (int)product.Status
            }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminProductCreatedDto>.Success(new AdminProductCreatedDto(
            product.Id, product.Title, product.Slug, product.Description,
            product.Brand, (int)product.Status, product.CreatedAt));
    }
}
