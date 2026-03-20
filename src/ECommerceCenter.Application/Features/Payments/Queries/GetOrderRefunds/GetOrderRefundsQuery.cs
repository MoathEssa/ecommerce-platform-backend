using ECommerceCenter.Application.Abstractions.DTOs.Payments;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Payments.Queries.GetOrderRefunds;

public record GetOrderRefundsQuery(int OrderId) : IRequest<Result<IReadOnlyList<RefundDto>>>;
