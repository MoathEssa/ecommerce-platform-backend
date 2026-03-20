using ECommerceCenter.Application.Abstractions.DTOs.Dashboard;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Dashboard.Queries;

public class GetDashboardSummaryQueryHandler(IDashboardService dashboardService)
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    public async Task<Result<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken ct)
    {
        var summary = await dashboardService.GetSummaryAsync(request.Days, ct);
        return Result<DashboardSummaryDto>.Success(summary);
    }
}
