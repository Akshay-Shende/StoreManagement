using Microsoft.AspNetCore.Mvc;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(IInventoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InventoryResponse>>> GetCurrent(CancellationToken cancellationToken) => Ok(await service.GetCurrentAsync(cancellationToken));

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryResponse>>> GetLowStock(CancellationToken cancellationToken) => Ok(await service.GetLowStockAsync(cancellationToken));

    [HttpGet("expiring")]
    public async Task<ActionResult<IReadOnlyCollection<ExpiringBatchResponse>>> GetExpiring([FromQuery] int days = 30, CancellationToken cancellationToken = default) => Ok(await service.GetExpiringAsync(days, cancellationToken));

    [HttpGet("expired")]
    public async Task<ActionResult<IReadOnlyCollection<ExpiringBatchResponse>>> GetExpired(CancellationToken cancellationToken) => Ok(await service.GetExpiredAsync(cancellationToken));

    [HttpPost("adjustments")]
    public async Task<ActionResult<StockAdjustmentResponse>> CreateAdjustment(CreateStockAdjustmentRequest request, CancellationToken cancellationToken) => Ok(await service.CreateAdjustmentAsync(request, cancellationToken));

    [HttpPost("adjustments/{id:guid}/approve")]
    public async Task<ActionResult<StockAdjustmentResponse>> ApproveAdjustment(Guid id, ApproveStockAdjustmentRequest request, [FromHeader(Name = "X-User")] string? createdBy, CancellationToken cancellationToken) =>
        (await service.ApproveAdjustmentAsync(id, request, createdBy ?? "system", cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost("batches/{id:guid}/expire")]
    public async Task<ActionResult<ExpiringBatchResponse>> ExpireBatch(Guid id, [FromHeader(Name = "X-User")] string? createdBy, CancellationToken cancellationToken) =>
        (await service.ExpireBatchAsync(id, createdBy ?? "system", cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpPost("returns")]
    public async Task<ActionResult<InventoryResponse>> RecordReturn(InventoryMovementRequest request, [FromHeader(Name = "X-User")] string? createdBy, CancellationToken cancellationToken) =>
        Ok(await service.RecordMovementAsync(InventoryTransactionType.Return, request, createdBy ?? "system", cancellationToken));

    [HttpPost("damage")]
    public async Task<ActionResult<InventoryResponse>> RecordDamage(InventoryMovementRequest request, [FromHeader(Name = "X-User")] string? createdBy, CancellationToken cancellationToken) =>
        Ok(await service.RecordMovementAsync(InventoryTransactionType.Damage, request, createdBy ?? "system", cancellationToken));
}
