using System.Security.Claims;
using ECommerceCenter.Application.Abstractions.DTOs.Admin;
using ECommerceCenter.Application.Features.Coupons.Commands.CreateCoupon;
using ECommerceCenter.Application.Features.Coupons.Commands.DeactivateCoupon;
using ECommerceCenter.Application.Features.Coupons.Commands.UpdateCoupon;
using ECommerceCenter.Application.Features.Coupons.Queries.GetCouponDetail;
using ECommerceCenter.Application.Features.Coupons.Queries.GetCoupons;
using ECommerceCenter.Application.Features.Coupons.Queries.GetCouponUsages;
using MediatR;
using ECommerceCenter.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[Route("api/v1/coupons")]
public class CouponsController(IMediator mediator) : AppController(mediator)
{
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateCoupon(
        [FromBody] CreateCouponBody body,
        CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var command = new CreateCouponCommand(
            body.Code, body.DiscountType, body.DiscountValue,
            body.MinOrderAmount, body.MaxDiscountAmount,
            body.UsageLimit, body.PerUserLimit, body.IsActive,
            body.StartsAt, body.ExpiresAt,
            body.ApplicableCategories ?? [],
            body.ApplicableProducts ?? [],
            body.ApplicableVariants ?? [],
            userId);
        return HandleResult(await Mediator.Send(command, ct));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCoupon(
        int id,
        [FromBody] UpdateCouponBody body,
        CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var command = new UpdateCouponCommand(
            id, body.Code, body.DiscountType, body.DiscountValue,
            body.MinOrderAmount, body.MaxDiscountAmount,
            body.UsageLimit, body.PerUserLimit, body.IsActive,
            body.StartsAt, body.ExpiresAt,
            body.ApplicableCategories ?? [],
            body.ApplicableProducts ?? [],
            body.ApplicableVariants ?? [],
            userId);
        return HandleResult(await Mediator.Send(command, ct));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateCoupon(int id, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        return HandleResult(await Mediator.Send(new DeactivateCouponCommand(id, userId), ct));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetCoupons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "newest",
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(
            new GetCouponsQuery(page, pageSize, search, isActive, sortBy), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCouponDetail(int id, CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetCouponDetailQuery(id), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:int}/usages")]
    public async Task<IActionResult> GetCouponUsages(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetCouponUsagesQuery(id, page, pageSize), ct));
}
