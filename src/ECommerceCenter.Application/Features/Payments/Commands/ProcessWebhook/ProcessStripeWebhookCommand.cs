using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Commands.ProcessWebhook;

/// <summary>
/// Carries the raw webhook body and Stripe signature header.
/// The handler verifies the signature and routes by event type.
/// </summary>
public record ProcessStripeWebhookCommand(
    string RawBody,
    string StripeSignature) : IRequest<Result>;
