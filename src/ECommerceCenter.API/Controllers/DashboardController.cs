using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

[Route("api/v1/admin/dashboard")]
[Authorize(Roles = Roles.Admin)]
public class DashboardController(IMediator mediator) : AppController(mediator)
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int days = 30,
        CancellationToken ct = default)
        => HandleResult(await Mediator.Send(new GetDashboardSummaryQuery(days), ct));
}
