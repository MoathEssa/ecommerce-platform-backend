using System.Data;
using System.Text.Json;
using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Common.Errors;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Inventory;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Application.Features.Inventory.Commands.CreateAdjustment;

public class CreateAdjustmentCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IInventoryAdjustmentRepository adjustmentRepository,
    IAuditLogRepository auditLogRepository,
    IEfUnitOfWork unitOfWork,
    ILogger<CreateAdjustmentCommandHandler> logger)
    : IRequestHandler<CreateAdjustmentCommand, Result<InventoryAdjustmentResultDto>>
{
    private const int MaxRetries = 3;

    public async Task<Result<InventoryAdjustmentResultDto>> Handle(
        CreateAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        for (var retry = 0; retry < MaxRetries; retry++)
        {
            var item = await inventoryItemRepository.GetByVariantIdAsync(request.VariantId, cancellationToken);
            if (item is null)
                return Result<InventoryAdjustmentResultDto>.NotFound("InventoryItem", request.VariantId);

            var validationResult = ValidateAdjustment(item, request.Delta);
            if (validationResult is not null)
                return validationResult;

            var now = DateTime.UtcNow;
            var beforeOnHand = item.OnHand;

            try
            {
                item.OnHand += request.Delta;
                item.UpdatedAt = now;

                var adjustment = new InventoryAdjustment
                {
                    VariantId = request.VariantId,
                    Delta = request.Delta,
                    Reason = request.Reason,
                    ActorId = request.ActorId,
                    CreatedAt = now
                };

                await adjustmentRepository.AddAsync(adjustment, cancellationToken);

                var auditLog = new AuditLog
                {
                    ActorId = request.ActorId,
                    ActorType = ActorType.Admin,
                    Action = "Inventory.Adjust",
                    EntityType = "InventoryItem",
                    EntityId = request.VariantId,
                    BeforeJson = JsonSerializer.Serialize(new { onHand = beforeOnHand }),
                    AfterJson = JsonSerializer.Serialize(new { onHand = item.OnHand }),
                    CreatedAt = now
                };

                await auditLogRepository.AddAsync(auditLog, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<InventoryAdjustmentResultDto>.Success(
                    new InventoryAdjustmentResultDto(
                        item.VariantId,
                        item.OnHand,
                        item.OnHand,
                        new AdjustmentDto(
                            adjustment.Id,
                            adjustment.Delta,
                            adjustment.Reason,
                            adjustment.ActorId,
                            adjustment.CreatedAt)));
            }
            catch (DBConcurrencyException ex) when (retry < MaxRetries - 1)
            {
                logger.LogWarning(
                    ex,
                    "Concurrency conflict adjusting inventory for variant {VariantId}, retry {Retry}",
                    request.VariantId,
                    retry + 1);

                await unitOfWork.ClearTrackedChangesAsync(cancellationToken);
            }
        }

        return Result<InventoryAdjustmentResultDto>.Conflict(
            "Could not apply adjustment due to concurrent modifications. Please try again.");
    }

    private static Result<InventoryAdjustmentResultDto>? ValidateAdjustment(
        Domain.Entities.Inventory.InventoryItem item,
        int delta)
    {
        if (delta >= 0)
            return null;

        var newOnHand = item.OnHand + delta;

        if (newOnHand < 0)
        {
            return Result<InventoryAdjustmentResultDto>.Failure(
                Error.BusinessRule(
                    BusinessRuleCode.InventoryBelowZero,
                    $"Adjustment would make OnHand negative ({item.OnHand} + {delta} = {newOnHand})."),
                System.Net.HttpStatusCode.BadRequest);
        }

        return null;
    }
}