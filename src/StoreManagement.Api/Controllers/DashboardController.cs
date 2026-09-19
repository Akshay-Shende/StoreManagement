using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(CancellationToken cancellationToken) => Ok(await service.GetAsync(cancellationToken));
}
