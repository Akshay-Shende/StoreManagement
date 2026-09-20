using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public sealed class ReturnService(ISaleDataService saleData, IStoreOperationsDataService operations, IInventoryDataService inventoryData, IAuditService audit) : IReturnService
{
    public async Task<ReturnResponse> CreateAsync(CreateReturnRequest request, string createdBy, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0) throw new ArgumentException("At least one return item is required.");
        var condition = request.Condition.Trim();
        if (condition is not ("Good" or "Damaged")) throw new ArgumentException("Condition must be Good or Damaged.");
        var sale = await saleData.GetByIdAsync(request.SaleId, cancellationToken) ?? throw new KeyNotFoundException("Sale was not found.");
        if (sale.Status == SaleStatus.Cancelled) throw new InvalidOperationException("Cancelled sales cannot be returned.");
        if (!string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            var existing = await operations.QueryReturns().FirstOrDefaultAsync(x => x.ClientRequestId == request.ClientRequestId, cancellationToken);
            if (existing is not null) return Map(existing);
        }
        var previous = await operations.QueryReturns().Where(x => x.SaleId == request.SaleId && x.Status == "Completed").SelectMany(x => x.Items).ToListAsync(cancellationToken);
        var result = new Return { SaleId = request.SaleId, Reason = request.Reason.Trim(), Condition = condition, Status = "Completed", ClientRequestId = request.ClientRequestId?.Trim(), CreatedBy = createdBy };
        await operations.ExecuteInTransactionAsync(async ct =>
        {
            decimal totalRefund = 0;
            foreach (var input in request.Items)
            {
                if (input.Quantity <= 0) throw new ArgumentException("Return quantity must be greater than zero.");
                var saleItem = sale.Items.FirstOrDefault(x => x.SaleItemId == input.SaleItemId) ?? throw new KeyNotFoundException($"Sale item {input.SaleItemId} was not found.");
                var returned = previous.Where(x => x.SaleItemId == input.SaleItemId).Sum(x => x.Quantity);
                if (returned + input.Quantity > saleItem.Quantity) throw new InvalidOperationException($"Return quantity exceeds the remaining returnable quantity ({saleItem.Quantity - returned}).");
                var refund = input.Quantity * saleItem.UnitPrice;
                var ri = new ReturnItem { SaleItemId = saleItem.SaleItemId, Quantity = input.Quantity, RefundAmount = refund, BatchId = input.BatchId };
                result.Items.Add(ri);
                totalRefund += refund;

                if (condition == "Good")
                {
                    if (saleItem.BatchAllocations.Count > 0)
                    {
                        var left = input.Quantity;
                        var allocations = input.BatchId.HasValue ? saleItem.BatchAllocations.Where(x => x.BatchId == input.BatchId).ToList() : saleItem.BatchAllocations.ToList();
                        foreach (var allocation in allocations)
                        {
                            if (left <= 0) break;
                            var alreadyReturnedFromBatch = previous.Where(x => x.SaleItemId == saleItem.SaleItemId && x.BatchId == allocation.BatchId).Sum(x => x.Quantity);
                            var availableToReturn = Math.Max(0, allocation.Quantity - alreadyReturnedFromBatch);
                            var qty = Math.Min(left, availableToReturn);
                            if (qty <= 0) continue;
                            allocation.Batch.AvailableQuantity += qty;
                            ri.BatchId ??= allocation.BatchId;
                            await inventoryData.AddTransactionAsync(new InventoryTransaction { ProductId = saleItem.ProductId, BatchId = allocation.BatchId, TransactionType = InventoryTransactionType.Return, QuantityDelta = qty, ReferenceType = "Return", ReferenceId = request.SaleId, Reason = "Customer return - sellable", CreatedBy = createdBy }, ct);
                            left -= qty;
                        }
                        if (left > 0) throw new InvalidOperationException("The requested batch return cannot be reconciled against the original sale allocation.");
                    }
                    else
                    {
                        saleItem.Product.CurrentStock += input.Quantity;
                        await inventoryData.AddTransactionAsync(new InventoryTransaction { ProductId = saleItem.ProductId, TransactionType = InventoryTransactionType.Return, QuantityDelta = input.Quantity, ReferenceType = "Return", ReferenceId = request.SaleId, Reason = "Customer return - sellable", CreatedBy = createdBy }, ct);
                    }
                }
                // Damaged returns are recorded but are deliberately not added to sellable inventory.
            }
            result.RefundAmount = totalRefund;
            await operations.AddReturnAsync(result, ct);
            sale.Status = SaleStatus.Returned;
            await saleData.SaveChangesAsync(ct);
        }, cancellationToken);
        await audit.WriteAsync("Return", "Sale", request.SaleId.ToString(), createdBy, null, null, null, new { result.ReturnId, result.RefundAmount, result.Condition }, result.Reason, cancellationToken);
        return new ReturnResponse(result.ReturnId, result.SaleId, result.RefundAmount, result.Reason, result.Condition, result.Status, result.CreatedBy, result.CreatedAt, result.Items.Select(x => new ReturnItemResponse(x.ReturnItemId, x.SaleItemId, x.BatchId, x.Quantity, x.RefundAmount)).ToList());
    }

    private static ReturnResponse Map(Return x) => new(x.ReturnId, x.SaleId, x.RefundAmount, x.Reason, x.Condition, x.Status, x.CreatedBy, x.CreatedAt, x.Items.Select(i => new ReturnItemResponse(i.ReturnItemId, i.SaleItemId, i.BatchId, i.Quantity, i.RefundAmount)).ToList());
}
