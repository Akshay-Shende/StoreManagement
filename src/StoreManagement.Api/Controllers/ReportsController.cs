using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "Admin,Manager")]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<ReportResponse>> Summary([FromQuery] DateTimeOffset? fromUtc, [FromQuery] DateTimeOffset? toUtc, CancellationToken cancellationToken) => Ok(await service.GetAsync(fromUtc, toUtc, cancellationToken));
}
