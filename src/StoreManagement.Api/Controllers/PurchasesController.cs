using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/purchases")]
[Authorize(Roles = "Admin,Manager")]
public sealed class PurchasesController(IPurchaseService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        (await service.GetByIdAsync(id, cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost]
    public async Task<ActionResult<PurchaseResponse>> Create(CreatePurchaseRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.PurchaseId }, result);
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<ActionResult<PurchaseResponse>> Receive(
        Guid id,
        ReceivePurchaseRequest request,
        [FromHeader(Name = "X-User")] string? createdBy,
        CancellationToken cancellationToken)
    {
        var actor = !string.IsNullOrWhiteSpace(createdBy) ? createdBy : (User.Identity?.Name ?? "system");
        var result = await service.ReceiveAsync(id, request, actor, cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }
}
