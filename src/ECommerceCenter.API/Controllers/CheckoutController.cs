using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Features.Checkout.Commands.PlaceOrder;
using ECommerceCenter.Application.Features.Checkout.Queries.CalculateFreight;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[Route("api/v1/checkout")]
public class CheckoutController(IMediator mediator, ICurrentUserService currentUser)
    : AppController(mediator)
{
    /// <summary>
    /// Calculates available shipping options (carrier name, price, estimated days) for the
    /// given cart items and destination country. Call this before PlaceOrder so the customer
    /// can choose their preferred carrier.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("freight")]
    public async Task<IActionResult> CalculateFreight(
        [FromBody] CalculateFreightRequestBody body,
        CancellationToken ct)
    {
        var query = new CalculateFreightQuery(body.Items, body.EndCountryCode, body.Zip);
        var result = await Mediator.Send(query, ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequestBody body,
        CancellationToken ct)
    {
        // ── Idempotency key ───────────────────────────────────────────────────
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKeyHeader)
            || string.IsNullOrWhiteSpace(idempotencyKeyHeader))
        {
            return BadRequest(new { success = false, message = "Idempotency-Key header is required." });
        }

        var idempotencyKey = idempotencyKeyHeader.ToString();

        // ── Resolve caller identity ────────────────────────────────────────────
        int? userId = currentUser.IsAuthenticated ? currentUser.UserId : null;
        var email   = body.Email ?? (currentUser.IsAuthenticated ? currentUser.Email : null);

        var command = new PlaceOrderCommand(
            userId,
            email,
            idempotencyKey,
            body.ComputeHash(),
            body.Items.Select(i => i.ToDto()).ToList(),
            body.ShippingAddress.ToDto(),
            body.BillingAddress?.ToDto(),
            body.CouponCode);

        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }
}
