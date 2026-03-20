using ECommerceCenter.Application.Abstractions.DTOs.Dashboard;

namespace ECommerceCenter.Application.Abstractions.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int days, CancellationToken cancellationToken = default);
}
