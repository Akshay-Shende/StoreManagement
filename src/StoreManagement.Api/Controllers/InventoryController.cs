using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public sealed class InventoryController(IInventoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InventoryResponse>>> GetCurrent(CancellationToken cancellationToken) =>
        Ok(await service.GetCurrentAsync(cancellationToken));

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryResponse>>> GetLowStock(CancellationToken cancellationToken) =>
        Ok(await service.GetLowStockAsync(cancellationToken));

    [HttpGet("expiring")]
    public async Task<ActionResult<IReadOnlyCollection<ExpiringBatchResponse>>> GetExpiring(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default) =>
        Ok(await service.GetExpiringAsync(days, cancellationToken));

    [HttpGet("expired")]
    public async Task<ActionResult<IReadOnlyCollection<ExpiringBatchResponse>>> GetExpired(CancellationToken cancellationToken) =>
        Ok(await service.GetExpiredAsync(cancellationToken));

    [HttpPost("adjustments")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<StockAdjustmentResponse>> CreateAdjustment(
        CreateStockAdjustmentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.CreateAdjustmentAsync(request, cancellationToken));

    [HttpPost("adjustments/{id:long}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<StockAdjustmentResponse>> ApproveAdjustment(
        long id,
        ApproveStockAdjustmentRequest request,
        [FromHeader(Name = "X-User")] string? createdBy,
        CancellationToken cancellationToken)
    {
        var actor = !string.IsNullOrWhiteSpace(createdBy) ? createdBy : (User.Identity?.Name ?? "system");
        var result = await service.ApproveAdjustmentAsync(id, request, actor, cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("batches/{id:long}/expire")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ExpiringBatchResponse>> ExpireBatch(
        long id,
        [FromHeader(Name = "X-User")] string? createdBy,
        CancellationToken cancellationToken)
    {
        var actor = !string.IsNullOrWhiteSpace(createdBy) ? createdBy : (User.Identity?.Name ?? "system");
        var result = await service.ExpireBatchAsync(id, actor, cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("returns")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<InventoryResponse>> RecordReturn(
        InventoryMovementRequest request,
        [FromHeader(Name = "X-User")] string? createdBy,
        CancellationToken cancellationToken)
    {
        var actor = !string.IsNullOrWhiteSpace(createdBy) ? createdBy : (User.Identity?.Name ?? "system");
        return Ok(await service.RecordMovementAsync(InventoryTransactionType.Return, request, actor, cancellationToken));
    }

    [HttpPost("damage")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<InventoryResponse>> RecordDamage(
        InventoryMovementRequest request,
        [FromHeader(Name = "X-User")] string? createdBy,
        CancellationToken cancellationToken)
    {
        var actor = !string.IsNullOrWhiteSpace(createdBy) ? createdBy : (User.Identity?.Name ?? "system");
        return Ok(await service.RecordMovementAsync(InventoryTransactionType.Damage, request, actor, cancellationToken));
    }
}
