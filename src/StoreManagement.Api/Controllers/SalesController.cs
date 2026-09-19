using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Authorize(Roles = "Admin,Manager,Cashier")]
public sealed class SalesController(ISaleService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        (await service.GetByIdAsync(id, cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Create(
        CreateSaleRequest request,
        [FromHeader(Name = "X-User")] string? createdBy,
        CancellationToken cancellationToken)
    {
        var actor = !string.IsNullOrWhiteSpace(createdBy) ? createdBy : (User.Identity?.Name ?? "system");
        return Ok(await service.CreateAndCompleteAsync(request, actor, cancellationToken));
    }
}
