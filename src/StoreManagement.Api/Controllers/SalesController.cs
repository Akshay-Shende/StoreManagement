using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Api.Authentication;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Authorize(Roles = "Admin,Manager,Cashier")]
public sealed class SalesController(ISaleService service) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SaleResponse>> GetById(long id, CancellationToken cancellationToken) => (await service.GetByIdAsync(id, cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Create(CreateSaleRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        var result = await service.CreateInternalAsync(request, User.GetActorName(), idempotencyKey, User.IsInRole("Admin") || User.IsInRole("Manager"), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:long}/payments")]
    public async Task<ActionResult<PaymentResponse>> AddPayment(long id, CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.AddPaymentAsync(id, request, User.GetActorName(), cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult<SaleResponse>> Cancel(long id, CancellationToken cancellationToken)
    {
        var result = await service.CancelAsync(id, User.GetActorName(), cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }
}
