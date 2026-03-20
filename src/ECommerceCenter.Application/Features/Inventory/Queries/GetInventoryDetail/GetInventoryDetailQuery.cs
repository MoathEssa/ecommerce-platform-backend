using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Inventory.Queries.GetInventoryDetail;

public record GetInventoryDetailQuery(int VariantId) : IRequest<Result<InventoryDetailDto>>;
