using ECommerceCenter.Application.Features.Payments.Commands.CreateRefund;
using ECommerceCenter.Application.Features.Payments.Commands.ProcessWebhook;
using ECommerceCenter.Application.Features.Payments.Queries.GetOrderRefunds;
using ECommerceCenter.Application.Features.Payments.Queries.GetStripeChargeById;
using ECommerceCenter.Application.Features.Payments.Queries.GetStripeCharges;
using MediatR;
using ECommerceCenter.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[Route("api/v1/payments")]
public class PaymentsController(IMediator mediator) : AppController(mediator)
{
    /// <summary>
    /// Stripe webhook endpoint. Receives signed events from Stripe.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("webhook/{provider}")]
    public async Task<IActionResult> Webhook(
        [FromRoute] string provider,
        CancellationToken ct)
    {
        if (!string.Equals(provider, "stripe", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { success = false, message = "Unsupported payment provider." });

        // Read raw body (required for Stripe signature verification)
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader)
            || string.IsNullOrWhiteSpace(signatureHeader))
        {
            return BadRequest(new { success = false, message = "Missing Stripe-Signature header." });
        }

        var command = new ProcessStripeWebhookCommand(rawBody, signatureHeader.ToString());
        var result = await Mediator.Send(command, ct);

        return HandleResult(result);
    
    }

    // ── Admin — Refunds ────────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("/api/v1/admin/orders/{orderId:int}/refunds")]
    public async Task<IActionResult> CreateRefund(
        int orderId,
        [FromBody] CreateRefundBody body,
        CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var command = new CreateRefundCommand(orderId, body.Amount, body.Reason, userId);
        var result = await Mediator.Send(command, ct);

        return HandleResult(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("/api/v1/admin/orders/{orderId:int}/refunds")]
    public async Task<IActionResult> GetOrderRefunds(int orderId, CancellationToken ct)
        => HandleResult(await Mediator.Send(new GetOrderRefundsQuery(orderId), ct));

    // ── Admin — Stripe Charges ─────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("/api/v1/admin/payments/charges")]
    public async Task<IActionResult> GetCharges(
        [FromQuery] int limit = 20,
        [FromQuery] string? startingAfter = null,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetStripeChargesQuery(limit, startingAfter), ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("/api/v1/admin/payments/charges/{chargeId}")]
    public async Task<IActionResult> GetCharge(string chargeId, CancellationToken ct)
        => HandleResult(await Mediator.Send(new GetStripeChargeByIdQuery(chargeId), ct));
}

public record CreateRefundBody(decimal Amount, string? Reason);
