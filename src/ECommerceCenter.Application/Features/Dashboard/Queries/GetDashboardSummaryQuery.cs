using ECommerceCenter.Application.Abstractions.DTOs.Dashboard;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Dashboard.Queries;

/// <summary>
/// Retrieves a full dashboard summary.
/// <paramref name="Days"/> controls how far back the "current period" spans (default 30).
/// </summary>
public record GetDashboardSummaryQuery(int Days = 30) : IRequest<Result<DashboardSummaryDto>>;
