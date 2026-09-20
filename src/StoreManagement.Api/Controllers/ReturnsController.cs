using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Api.Authentication;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/returns")]
[Authorize(Roles = "Admin,Manager,Cashier")]
public sealed class ReturnsController(IReturnService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ReturnResponse>> Create(CreateReturnRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        if (request.ClientRequestId is null && !string.IsNullOrWhiteSpace(idempotencyKey)) request = request with { ClientRequestId = idempotencyKey };
        return Ok(await service.CreateAsync(request, User.GetActorName(), cancellationToken));
    }
}
