using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public sealed class InventoryService(IInventoryDataService data, IAuditService audit) : IInventoryService
{
    public async Task<IReadOnlyCollection<InventoryResponse>> GetCurrentAsync(CancellationToken cancellationToken) =>
        await data.QueryProducts().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new InventoryResponse(x.ProductId, x.Name, x.SKU, x.Unit, x.CurrentStock, x.ReorderLevel, x.ReorderQuantity, x.CurrentStock <= x.ReorderLevel)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<InventoryResponse>> GetLowStockAsync(CancellationToken cancellationToken) =>
        await data.QueryProducts().AsNoTracking().Where(x => x.IsActive && x.CurrentStock <= x.ReorderLevel).OrderBy(x => x.CurrentStock).Select(x => new InventoryResponse(x.ProductId, x.Name, x.SKU, x.Unit, x.CurrentStock, x.ReorderLevel, x.ReorderQuantity, true)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ExpiringBatchResponse>> GetExpiringAsync(int days, CancellationToken cancellationToken)
    {
        if (days <= 0) throw new ArgumentException("Days must be greater than zero.");
        var today = DateOnly.FromDateTime(DateTime.UtcNow); var end = today.AddDays(days);
        var rows = await data.QueryBatches().AsNoTracking().Where(x => x.IsSellable && x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value >= today && x.ExpiryDate.Value <= end).OrderBy(x => x.ExpiryDate).Select(x => new { x.BatchId, x.ProductId, ProductName = x.Product.Name, x.BatchNumber, x.AvailableQuantity, x.ExpiryDate }).ToListAsync(cancellationToken);
        return rows.Select(x => new ExpiringBatchResponse(x.BatchId, x.ProductId, x.ProductName, x.BatchNumber, x.AvailableQuantity, x.ExpiryDate, x.ExpiryDate!.Value.DayNumber - today.DayNumber)).ToList();
    }

    public async Task<IReadOnlyCollection<ExpiringBatchResponse>> GetExpiredAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rows = await data.QueryBatches().AsNoTracking().Where(x => x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value < today).OrderBy(x => x.ExpiryDate).Select(x => new { x.BatchId, x.ProductId, ProductName = x.Product.Name, x.BatchNumber, x.AvailableQuantity, x.ExpiryDate }).ToListAsync(cancellationToken);
        return rows.Select(x => new ExpiringBatchResponse(x.BatchId, x.ProductId, x.ProductName, x.BatchNumber, x.AvailableQuantity, x.ExpiryDate, x.ExpiryDate!.Value.DayNumber - today.DayNumber)).ToList();
    }

    public async Task<IReadOnlyCollection<StockAdjustmentResponse>> GetAdjustmentsAsync(CancellationToken cancellationToken) =>
        await data.QueryAdjustments().AsNoTracking().OrderByDescending(x => x.CreatedAt).Select(x => new StockAdjustmentResponse(x.AdjustmentId, x.ProductId, x.Product.Name, x.SystemQuantity, x.PhysicalQuantity, x.Difference, x.Reason, x.CreatedBy, x.ApprovedBy, x.IsApproved, x.CreatedAt, x.ApprovedAt)).ToListAsync(cancellationToken);

    public async Task<StockAdjustmentResponse> CreateAdjustmentAsync(CreateStockAdjustmentRequest request, string createdBy, string? clientRequestId, CancellationToken cancellationToken)
    {
        var product = await data.GetProductAsync(request.ProductId, cancellationToken) ?? throw new KeyNotFoundException("Product was not found.");
        if (request.PhysicalQuantity < 0) throw new ArgumentException("Physical quantity cannot be negative.");
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new ArgumentException("Adjustment reason is required.");
        if (!string.IsNullOrWhiteSpace(clientRequestId))
        {
            var duplicate = await data.QueryAdjustments().AsNoTracking().FirstOrDefaultAsync(x => x.ClientRequestId == clientRequestId, cancellationToken);
            if (duplicate is not null) return Map(duplicate, product.Name);
        }
        var adjustment = new StockAdjustment { ProductId = product.ProductId, SystemQuantity = product.CurrentStock, PhysicalQuantity = request.PhysicalQuantity, Difference = request.PhysicalQuantity - product.CurrentStock, Reason = request.Reason.Trim(), CreatedBy = createdBy, IsApproved = false, ClientRequestId = clientRequestId?.Trim() };
        await data.AddAdjustmentAsync(adjustment, cancellationToken);
        await data.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("Created", "StockAdjustment", adjustment.AdjustmentId.ToString(), createdBy, null, null, null, new { adjustment.SystemQuantity, adjustment.PhysicalQuantity, adjustment.Difference }, adjustment.Reason, cancellationToken);
        return Map(adjustment, product.Name);
    }

    public async Task<StockAdjustmentResponse?> ApproveAdjustmentAsync(long id, ApproveStockAdjustmentRequest request, string createdBy, CancellationToken cancellationToken)
    {
        var adjustment = await data.GetAdjustmentAsync(id, cancellationToken);
        if (adjustment is null) return null;
        if (adjustment.IsApproved) throw new InvalidOperationException("Adjustment is already approved.");
        if (string.Equals(adjustment.CreatedBy, createdBy, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("A stock adjustment must be approved by a different user.");
        var before = new { adjustment.SystemQuantity, adjustment.PhysicalQuantity, adjustment.Difference, adjustment.IsApproved };
        await data.ExecuteInTransactionAsync(async ct =>
        {
            var product = await data.GetProductAsync(adjustment.ProductId, ct) ?? throw new KeyNotFoundException("Product was not found.");
            var difference = adjustment.PhysicalQuantity - product.CurrentStock;
            adjustment.SystemQuantity = product.CurrentStock;
            adjustment.Difference = difference;
            product.CurrentStock = adjustment.PhysicalQuantity;
            adjustment.ApprovedBy = createdBy;
            adjustment.ApprovedAt = DateTimeOffset.UtcNow;
            adjustment.IsApproved = true;
            await data.AddTransactionAsync(new InventoryTransaction { ProductId = product.ProductId, TransactionType = InventoryTransactionType.Adjustment, QuantityDelta = difference, ReferenceType = "StockAdjustment", ReferenceId = adjustment.AdjustmentId, Reason = adjustment.Reason, CreatedBy = createdBy }, ct);
        }, cancellationToken);
        await audit.WriteAsync("Approved", "StockAdjustment", id.ToString(), createdBy, null, null, before, new { adjustment.SystemQuantity, adjustment.PhysicalQuantity, adjustment.Difference, adjustment.IsApproved }, adjustment.Reason, cancellationToken);
        return await GetAdjustmentResponseAsync(id, cancellationToken);
    }

    public async Task<ExpiringBatchResponse?> ExpireBatchAsync(long batchId, string createdBy, CancellationToken cancellationToken)
    {
        var batch = await data.GetBatchAsync(batchId, cancellationToken); if (batch is null) return null;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!batch.ExpiryDate.HasValue || batch.ExpiryDate.Value >= today) throw new InvalidOperationException("Batch is not expired.");
        if (batch.AvailableQuantity <= 0 || !batch.IsSellable) throw new InvalidOperationException("Batch has no sellable quantity to expire.");
        await data.ExecuteInTransactionAsync(async ct =>
        {
            var expiredQty = batch.AvailableQuantity; batch.AvailableQuantity = 0; batch.IsSellable = false;
            var product = await data.GetProductAsync(batch.ProductId, ct) ?? throw new KeyNotFoundException("Product was not found.");
            if (product.CurrentStock < expiredQty) throw new InvalidOperationException("Current stock is inconsistent with batch quantity.");
            product.CurrentStock -= expiredQty;
            await data.AddTransactionAsync(new InventoryTransaction { ProductId = product.ProductId, BatchId = batch.BatchId, TransactionType = InventoryTransactionType.Expired, QuantityDelta = -expiredQty, ReferenceType = "Expiry", ReferenceId = batch.BatchId, Reason = "Expired stock removed from sellable inventory", CreatedBy = createdBy }, ct);
        }, cancellationToken);
        await audit.WriteAsync("Expired", "Batch", batchId.ToString(), createdBy, null, null, null, new { batch.ProductId, batch.BatchNumber }, "Batch expired", cancellationToken);
        return new ExpiringBatchResponse(batch.BatchId, batch.ProductId, (await data.QueryProducts().AsNoTracking().Where(x => x.ProductId == batch.ProductId).Select(x => x.Name).FirstAsync(cancellationToken)), batch.BatchNumber, 0, batch.ExpiryDate, batch.ExpiryDate.Value.DayNumber - today.DayNumber);
    }

    public async Task<InventoryResponse?> RecordMovementAsync(InventoryTransactionType movementType, InventoryMovementRequest request, string createdBy, CancellationToken cancellationToken)
    {
        if (movementType is not (InventoryTransactionType.Return or InventoryTransactionType.Damage)) throw new ArgumentException("Only RETURN and DAMAGE movements can be recorded through this endpoint.");
        if (request.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new ArgumentException("Reason is required.");
        var product = await data.GetProductAsync(request.ProductId, cancellationToken) ?? throw new KeyNotFoundException("Product was not found.");
        if (movementType == InventoryTransactionType.Damage && product.CurrentStock < request.Quantity) throw new InvalidOperationException($"Insufficient stock for damage. Available: {product.CurrentStock}.");
        await data.ExecuteInTransactionAsync(async ct =>
        {
            Batch? batch = null;
            if (product.RequiresBatchTracking && !request.BatchId.HasValue) throw new ArgumentException("BatchId is required for a batch-tracked product.");
            if (request.BatchId.HasValue)
            {
                batch = await data.GetBatchAsync(request.BatchId.Value, ct) ?? throw new KeyNotFoundException("Batch was not found.");
                if (batch.ProductId != product.ProductId) throw new InvalidOperationException("Batch does not belong to the selected product.");
                if (movementType == InventoryTransactionType.Damage && batch.AvailableQuantity < request.Quantity) throw new InvalidOperationException("Insufficient quantity in the selected batch.");
            }
            var delta = movementType == InventoryTransactionType.Return ? request.Quantity : -request.Quantity;
            product.CurrentStock += delta;
            if (product.CurrentStock < 0) throw new InvalidOperationException("Inventory cannot become negative.");
            if (batch is not null)
            {
                batch.AvailableQuantity += delta;
                if (batch.AvailableQuantity < 0) throw new InvalidOperationException("Batch inventory cannot become negative.");
                if (movementType == InventoryTransactionType.Return && batch.ExpiryDate.HasValue && batch.ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow)) batch.IsSellable = false;
            }
            await data.AddTransactionAsync(new InventoryTransaction { ProductId = product.ProductId, BatchId = batch?.BatchId, TransactionType = movementType, QuantityDelta = delta, ReferenceType = movementType.ToString(), ReferenceId = product.ProductId, Reason = request.Reason.Trim(), CreatedBy = createdBy }, ct);
        }, cancellationToken);
        await audit.WriteAsync(movementType.ToString(), "Product", product.ProductId.ToString(), createdBy, null, null, null, new { product.CurrentStock }, request.Reason, cancellationToken);
        return new InventoryResponse(product.ProductId, product.Name, product.SKU, product.Unit, product.CurrentStock, product.ReorderLevel, product.ReorderQuantity, product.CurrentStock <= product.ReorderLevel);
    }

    public async Task<IReadOnlyCollection<InventoryReconciliationResponse>> ReconcileAsync(CancellationToken cancellationToken)
    {
        var products = await data.QueryProducts().AsNoTracking().Where(x => x.IsActive).Select(x => new { x.ProductId, x.Name, x.CurrentStock }).ToListAsync(cancellationToken);
        var ids = products.Select(x => x.ProductId).ToArray();
        var batches = await data.QueryBatches().AsNoTracking().Where(x => ids.Contains(x.ProductId)).GroupBy(x => x.ProductId).Select(g => new { ProductId = g.Key, Stock = g.Sum(x => x.AvailableQuantity) }).ToDictionaryAsync(x => x.ProductId, cancellationToken);
        var ledger = await data.QueryTransactions().AsNoTracking().Where(x => ids.Contains(x.ProductId)).GroupBy(x => x.ProductId).Select(g => new { ProductId = g.Key, Delta = g.Sum(x => x.QuantityDelta) }).ToDictionaryAsync(x => x.ProductId, cancellationToken);
        return products.Select(x => { var bs = batches.GetValueOrDefault(x.ProductId); var ls = ledger.GetValueOrDefault(x.ProductId); return new InventoryReconciliationResponse(x.ProductId, x.Name, x.CurrentStock, bs?.Stock ?? 0, ls?.Delta ?? 0, x.CurrentStock - (bs?.Stock ?? 0), x.CurrentStock - (ls?.Delta ?? 0), Math.Abs(x.CurrentStock - (bs?.Stock ?? 0)) < 0.001m && Math.Abs(x.CurrentStock - (ls?.Delta ?? 0)) < 0.001m); }).ToList();
    }

    public async Task<IReadOnlyCollection<PurchaseSuggestionResponse>> GetPurchaseSuggestionsAsync(CancellationToken cancellationToken) =>
        await data.QueryProducts().AsNoTracking().Where(x => x.IsActive && x.CurrentStock <= x.ReorderLevel).OrderBy(x => x.CurrentStock).Select(x => new PurchaseSuggestionResponse(x.ProductId, x.Name, x.CurrentStock, x.ReorderLevel, x.ReorderQuantity, "Current stock is at or below reorder level.")).ToListAsync(cancellationToken);

    private async Task<StockAdjustmentResponse?> GetAdjustmentResponseAsync(long id, CancellationToken cancellationToken) =>
        await data.QueryAdjustments().AsNoTracking().Where(x => x.AdjustmentId == id).Select(x => new StockAdjustmentResponse(x.AdjustmentId, x.ProductId, x.Product.Name, x.SystemQuantity, x.PhysicalQuantity, x.Difference, x.Reason, x.CreatedBy, x.ApprovedBy, x.IsApproved, x.CreatedAt, x.ApprovedAt)).FirstOrDefaultAsync(cancellationToken);

    private static StockAdjustmentResponse Map(StockAdjustment x, string productName) => new(x.AdjustmentId, x.ProductId, productName, x.SystemQuantity, x.PhysicalQuantity, x.Difference, x.Reason, x.CreatedBy, x.ApprovedBy, x.IsApproved, x.CreatedAt, x.ApprovedAt);
}
