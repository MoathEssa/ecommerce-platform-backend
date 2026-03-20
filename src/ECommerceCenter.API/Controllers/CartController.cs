using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Features.Cart.Commands.AddCartItem;
using ECommerceCenter.Application.Features.Cart.Commands.ApplyCoupon;
using ECommerceCenter.Application.Features.Cart.Commands.ClearCart;
using ECommerceCenter.Application.Features.Cart.Commands.RemoveCartItem;
using ECommerceCenter.Application.Features.Cart.Commands.RemoveCoupon;
using ECommerceCenter.Application.Features.Cart.Commands.SendCartReminderEmail;
using ECommerceCenter.Application.Features.Cart.Commands.UpdateCartItem;
using ECommerceCenter.Application.Features.Cart.Queries.GetAdminCarts;
using ECommerceCenter.Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[Route("api/v1/cart")]
public class CartController(
    IMediator mediator,
    ICurrentUserService currentUser,
    IWebHostEnvironment env)
    : AppController(mediator)
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] int? userId, CancellationToken ct)
    {
        int? resolvedUserId;
        string? resolvedSessionId;

        // Admin can look up any cart by userId query param
        if (userId.HasValue && currentUser.IsAuthenticated && User.IsInRole(Roles.Admin))
        {
            resolvedUserId    = userId.Value;
            resolvedSessionId = null;
        }
        else
        {
            (resolvedUserId, resolvedSessionId) = ResolveCartIdentifiers();
        }

        var result = await Mediator.Send(new GetCartQuery(resolvedUserId, resolvedSessionId), ct);
        return HandleResult(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin/carts")]
    public async Task<IActionResult> GetAdminCarts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetAdminCartsQuery(page, pageSize, search, status), ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemBody body,
        CancellationToken ct)
    {
        var sessionId = currentUser.IsAuthenticated
            ? null
            : CartCookieHelper.EnsureSessionId(Request.Cookies, Response.Cookies, !env.IsDevelopment());

        var userId = currentUser.IsAuthenticated ? currentUser.UserId : (int?)null;

        var result = await Mediator.Send(new AddCartItemCommand(userId, sessionId, body.VariantId, body.Quantity), ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPut("items/{itemId:int}")]
    public async Task<IActionResult> UpdateItem(
        int itemId,
        [FromBody] UpdateCartItemBody body,
        CancellationToken ct)
    {
        var (userId, sessionId) = ResolveCartIdentifiers();
        var result = await Mediator.Send(
            new UpdateCartItemCommand(userId, sessionId, itemId, body.Quantity), ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int itemId, CancellationToken ct)
    {
        var (userId, sessionId) = ResolveCartIdentifiers();
        var result = await Mediator.Send(
            new RemoveCartItemCommand(userId, sessionId, itemId), ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        var (userId, sessionId) = ResolveCartIdentifiers();
        var result = await Mediator.Send(new ClearCartCommand(userId, sessionId), ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("coupon")]
    public async Task<IActionResult> ApplyCoupon(
        [FromBody] ApplyCouponBody body,
        CancellationToken ct)
    {
        var (userId, sessionId) = ResolveCartIdentifiers();
        var result = await Mediator.Send(
            new ApplyCouponCommand(userId, sessionId, body.Code), ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct)
    {
        var (userId, sessionId) = ResolveCartIdentifiers();
        var result = await Mediator.Send(new RemoveCouponCommand(userId, sessionId), ct);
        return HandleResult(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin/send-reminder")]
    public async Task<IActionResult> SendReminderEmail(
        [FromBody] SendCartReminderEmailCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private (int? userId, string? sessionId) ResolveCartIdentifiers()
    {
        if (currentUser.IsAuthenticated)
            return (currentUser.UserId, null);

        return (null, CartCookieHelper.GetSessionId(Request.Cookies));
    }
}
