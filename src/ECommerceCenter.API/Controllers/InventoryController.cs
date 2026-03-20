using System.Security.Claims;
using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Features.Inventory.Commands.CreateAdjustment;
using ECommerceCenter.Application.Features.Inventory.Queries.GetAdjustmentHistory;
using ECommerceCenter.Application.Features.Inventory.Queries.GetInventoryDetail;
using ECommerceCenter.Application.Features.Inventory.Queries.GetInventoryList;
using MediatR;
using ECommerceCenter.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[Route("api/v1/inventory")]
public class InventoryController(IMediator mediator) : AppController(mediator)
{
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetInventory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? stockStatus = null,
        [FromQuery] string sortBy = "sku",
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new GetInventoryListQuery(page, pageSize, search, stockStatus, sortBy), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{variantId:int}")]
    public async Task<IActionResult> GetInventoryDetail(int variantId, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetInventoryDetailQuery(variantId), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{variantId:int}/adjustments")]
    public async Task<IActionResult> CreateAdjustment(
        int variantId,
        [FromBody] CreateAdjustmentBody body,
        CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        return HandleResult(await Mediator.Send(
            new CreateAdjustmentCommand(variantId, body.Delta, body.Reason, userId), ct));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{variantId:int}/adjustments")]
    public async Task<IActionResult> GetAdjustmentHistory(
        int variantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new GetAdjustmentHistoryQuery(variantId, page, pageSize), ct));
}
