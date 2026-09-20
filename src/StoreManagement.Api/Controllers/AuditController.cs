using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
[Authorize(Roles = "Admin,Manager")]
public sealed class AuditController(IAuditService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AuditLogResponse>>> Get([FromQuery] int take = 100, CancellationToken cancellationToken = default) => Ok(await service.GetAsync(take, cancellationToken));
}
