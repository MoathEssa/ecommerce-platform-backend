using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Commands.CreateRefund;

public record CreateRefundCommand(
    int OrderId,
    decimal Amount,
    string? Reason,
    int ActorId) : IRequest<Result<RefundDto>>;
