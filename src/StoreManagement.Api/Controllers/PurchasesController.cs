using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Api.Authentication;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/purchases")]
[Authorize(Roles = "Admin,Manager")]
public sealed class PurchasesController(IPurchaseService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PurchaseResponse>>> GetAll(CancellationToken cancellationToken) => Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PurchaseResponse>> GetById(long id, CancellationToken cancellationToken) => (await service.GetByIdAsync(id, cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost]
    public async Task<ActionResult<PurchaseResponse>> Create(CreatePurchaseRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, idempotencyKey, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.PurchaseId }, result);
    }

    [HttpPost("{id:long}/receive")]
    public async Task<ActionResult<PurchaseResponse>> Receive(long id, ReceivePurchaseRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        var result = await service.ReceiveAsync(id, request, User.GetActorName(), idempotencyKey, cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult<PurchaseResponse>> Cancel(long id, CancellationToken cancellationToken)
    {
        var result = await service.CancelAsync(id, User.GetActorName(), cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }
}
