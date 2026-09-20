using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Api.Authentication;
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
    public async Task<ActionResult<IReadOnlyCollection<InventoryResponse>>> GetCurrent(CancellationToken cancellationToken) => Ok(await service.GetCurrentAsync(cancellationToken));
    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryResponse>>> GetLowStock(CancellationToken cancellationToken) => Ok(await service.GetLowStockAsync(cancellationToken));
    [HttpGet("expiring")]
    public async Task<ActionResult<IReadOnlyCollection<ExpiringBatchResponse>>> GetExpiring([FromQuery] int days = 30, CancellationToken cancellationToken = default) => Ok(await service.GetExpiringAsync(days, cancellationToken));
    [HttpGet("expired")]
    public async Task<ActionResult<IReadOnlyCollection<ExpiringBatchResponse>>> GetExpired(CancellationToken cancellationToken) => Ok(await service.GetExpiredAsync(cancellationToken));
    [HttpGet("adjustments")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyCollection<StockAdjustmentResponse>>> GetAdjustments(CancellationToken cancellationToken) => Ok(await service.GetAdjustmentsAsync(cancellationToken));
    [HttpPost("adjustments")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<StockAdjustmentResponse>> CreateAdjustment(CreateStockAdjustmentRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken) => Ok(await service.CreateAdjustmentAsync(request, User.GetActorName(), idempotencyKey, cancellationToken));
    [HttpPost("adjustments/{id:long}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<StockAdjustmentResponse>> ApproveAdjustment(long id, ApproveStockAdjustmentRequest request, CancellationToken cancellationToken)
    { var result = await service.ApproveAdjustmentAsync(id, request, User.GetActorName(), cancellationToken); return result is not null ? Ok(result) : NotFound(); }
    [HttpPost("batches/{id:long}/expire")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ExpiringBatchResponse>> ExpireBatch(long id, CancellationToken cancellationToken)
    { var result = await service.ExpireBatchAsync(id, User.GetActorName(), cancellationToken); return result is not null ? Ok(result) : NotFound(); }
    [HttpPost("returns")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<InventoryResponse>> RecordReturn(InventoryMovementRequest request, CancellationToken cancellationToken) => Ok(await service.RecordMovementAsync(InventoryTransactionType.Return, request, User.GetActorName(), cancellationToken));
    [HttpPost("damage")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<InventoryResponse>> RecordDamage(InventoryMovementRequest request, CancellationToken cancellationToken) => Ok(await service.RecordMovementAsync(InventoryTransactionType.Damage, request, User.GetActorName(), cancellationToken));
    [HttpGet("reconciliation")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryReconciliationResponse>>> Reconciliation(CancellationToken cancellationToken) => Ok(await service.ReconcileAsync(cancellationToken));
    [HttpGet("purchase-suggestions")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyCollection<PurchaseSuggestionResponse>>> PurchaseSuggestions(CancellationToken cancellationToken) => Ok(await service.GetPurchaseSuggestionsAsync(cancellationToken));
}
