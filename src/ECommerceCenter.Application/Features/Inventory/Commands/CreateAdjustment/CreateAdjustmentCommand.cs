using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Inventory.Commands.CreateAdjustment;

public record CreateAdjustmentCommand(
    int VariantId,
    int Delta,
    string Reason,
    int ActorId) : IRequest<Result<InventoryAdjustmentResultDto>>;
