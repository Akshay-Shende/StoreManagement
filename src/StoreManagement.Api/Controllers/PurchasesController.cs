using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/purchases")]
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
    public async Task<ActionResult<PurchaseResponse>> Receive(Guid id, ReceivePurchaseRequest request, [FromHeader(Name = "X-User")] string? createdBy, CancellationToken cancellationToken) =>
        (await service.ReceiveAsync(id, request, createdBy ?? "system", cancellationToken)) is { } result ? Ok(result) : NotFound();
}
