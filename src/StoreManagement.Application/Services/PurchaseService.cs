using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public sealed class PurchaseService(
    IPurchaseDataService purchaseData,
    ISupplierDataService supplierData,
    IProductDataService productData,
    IInventoryDataService inventoryData,
    IStoreOperationsDataService operations,
    IAuditService audit) : IPurchaseService
{
    public async Task<IReadOnlyCollection<PurchaseResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var purchases = await purchaseData.GetAllAsync(cancellationToken);
        return purchases.Select(Map).ToList();
    }

    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, string? clientRequestId, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0) throw new ArgumentException("At least one purchase item is required.");
        if (string.IsNullOrWhiteSpace(clientRequestId) == false)
        {
            var existing = await purchaseData.GetAllAsync(cancellationToken);
            var duplicate = existing.FirstOrDefault(x => x.ClientRequestId == clientRequestId);
            if (duplicate is not null) return Map(duplicate);
        }
        var supplier = await supplierData.GetByIdAsync(request.SupplierId, cancellationToken) ?? throw new KeyNotFoundException("Supplier was not found.");
        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToArray();
        var products = await productData.Query().Where(x => productIds.Contains(x.ProductId) && x.IsActive).ToDictionaryAsync(x => x.ProductId, cancellationToken);
        if (products.Count != productIds.Length) throw new KeyNotFoundException("One or more products were not found or inactive.");
        var purchase = new Purchase { SupplierId = supplier.SupplierId, PurchaseDate = request.PurchaseDate ?? DateTimeOffset.UtcNow, Status = PurchaseStatus.Draft, ClientRequestId = clientRequestId?.Trim(), Supplier = supplier };
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0) throw new ArgumentException("Purchase quantity must be greater than zero.");
            if (item.UnitPrice < 0) throw new ArgumentException("Unit price cannot be negative.");
            purchase.Items.Add(new PurchaseItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
        }
        purchase.TotalAmount = purchase.Items.Sum(x => x.Quantity * x.UnitPrice);
        await purchaseData.AddAsync(purchase, cancellationToken);
        await purchaseData.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("Created", "Purchase", purchase.PurchaseId.ToString(), "system", null, null, null, new { purchase.PurchaseId, purchase.SupplierId, purchase.TotalAmount }, null, cancellationToken);
        return Map(purchase);
    }

    public async Task<PurchaseResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var purchase = await purchaseData.GetByIdAsync(id, cancellationToken);
        return purchase is null ? null : Map(purchase);
    }

    public async Task<PurchaseResponse?> ReceiveAsync(long id, ReceivePurchaseRequest request, string createdBy, string? clientRequestId, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0) throw new ArgumentException("At least one receipt item is required.");
        var purchase = await purchaseData.GetByIdAsync(id, cancellationToken);
        if (purchase is null) return null;
        if (purchase.Status is PurchaseStatus.Cancelled or PurchaseStatus.Received) throw new InvalidOperationException("Purchase cannot be received in its current status.");
        var normalizedRequestId = clientRequestId?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedRequestId))
        {
            var existingReceipt = await operations.QueryGoodsReceipts()
                .FirstOrDefaultAsync(x => x.PurchaseId == id && x.ClientRequestId == normalizedRequestId, cancellationToken);
            if (existingReceipt is not null) return await GetByIdAsync(id, cancellationToken);
        }
        await purchaseData.ExecuteInTransactionAsync(async ct =>
        {
            var receipt = new GoodsReceipt { PurchaseId = purchase.PurchaseId, ClientRequestId = normalizedRequestId, ReceivedBy = createdBy, SupplierInvoiceNumber = request.SupplierInvoiceNumber, Notes = request.Notes };
            foreach (var input in request.Items)
            {
                var item = purchase.Items.FirstOrDefault(x => x.PurchaseItemId == input.PurchaseItemId) ?? throw new KeyNotFoundException($"Purchase item {input.PurchaseItemId} was not found.");
                var remaining = item.Quantity - item.ReceivedQuantity;
                var received = input.ReceivedQuantity;
                if (received <= 0) continue;
                if (received > remaining) throw new InvalidOperationException($"Received quantity exceeds the remaining quantity ({remaining}) for purchase item {item.PurchaseItemId}.");
                if (input.ExpiryDate.HasValue && input.ManufacturingDate.HasValue && input.ExpiryDate.Value < input.ManufacturingDate.Value) throw new ArgumentException("Expiry date cannot be before manufacturing date.");
                var receiptItem = new GoodsReceiptItem { PurchaseItemId = item.PurchaseItemId, Quantity = received };
                receipt.Items.Add(receiptItem);
                Batch? batch = null;
                if (item.Product.RequiresBatchTracking)
                {
                    if (string.IsNullOrWhiteSpace(input.BatchNumber)) throw new ArgumentException($"Batch number is required for {item.Product.Name}.");
                    batch = await inventoryData.QueryBatches().FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.BatchNumber == input.BatchNumber.Trim(), ct);
                    if (batch is null)
                    {
                        batch = new Batch { ProductId = item.ProductId, BatchNumber = input.BatchNumber.Trim(), ManufacturingDate = input.ManufacturingDate, ExpiryDate = input.ExpiryDate, ReceivedQuantity = 0, AvailableQuantity = 0, IsSellable = true };
                        await inventoryData.AddBatchAsync(batch, ct);
                        await inventoryData.SaveChangesAsync(ct);
                    }
                    else
                    {
                        if (input.ExpiryDate.HasValue && batch.ExpiryDate.HasValue && input.ExpiryDate.Value != batch.ExpiryDate.Value) throw new InvalidOperationException("Expiry date does not match the existing batch.");
                        if (input.ManufacturingDate.HasValue && batch.ManufacturingDate.HasValue && input.ManufacturingDate.Value != batch.ManufacturingDate.Value) throw new InvalidOperationException("Manufacturing date does not match the existing batch.");
                        if (!batch.IsSellable) throw new InvalidOperationException("The selected batch is not sellable.");
                    }
                    batch.ReceivedQuantity += received;
                    batch.AvailableQuantity += received;
                    batch.ManufacturingDate ??= input.ManufacturingDate;
                    batch.ExpiryDate ??= input.ExpiryDate;
                    receiptItem.Batches.Add(new GoodsReceiptItemBatch { BatchId = batch.BatchId, Quantity = received });
                    item.BatchId = batch.BatchId;
                }
                item.ReceivedQuantity += received;
                item.Product.CurrentStock += received;
                await inventoryData.AddTransactionAsync(new InventoryTransaction { ProductId = item.ProductId, BatchId = batch?.BatchId, TransactionType = InventoryTransactionType.Purchase, QuantityDelta = received, ReferenceType = "GoodsReceipt", ReferenceId = id, Reason = "Goods received", CreatedBy = createdBy }, ct);
            }
            if (receipt.Items.Count == 0) throw new ArgumentException("At least one positive received quantity is required.");
            await operations.AddGoodsReceiptAsync(receipt, ct);
            purchase.Status = purchase.Items.All(x => x.ReceivedQuantity >= x.Quantity) ? PurchaseStatus.Received : PurchaseStatus.PartiallyReceived;
            purchaseData.Update(purchase);
        }, cancellationToken);
        await audit.WriteAsync("Received", "Purchase", id.ToString(), createdBy, null, null, null, new { purchase.Status }, request.Notes, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PurchaseResponse?> CancelAsync(long id, string actor, CancellationToken cancellationToken)
    {
        var purchase = await purchaseData.GetByIdAsync(id, cancellationToken);
        if (purchase is null) return null;
        if (purchase.Status == PurchaseStatus.Received || purchase.Status == PurchaseStatus.Cancelled) throw new InvalidOperationException("Purchase cannot be cancelled in its current status.");
        if (purchase.Items.Any(x => x.ReceivedQuantity > 0)) throw new InvalidOperationException("A partially received purchase must be closed or completed before cancellation.");
        var before = purchase.Status.ToString();
        purchase.Status = PurchaseStatus.Cancelled;
        purchaseData.Update(purchase);
        await purchaseData.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("Cancelled", "Purchase", id.ToString(), actor, null, null, new { Status = before }, new { Status = purchase.Status.ToString() }, "Purchase cancelled", cancellationToken);
        return Map(purchase);
    }

    private static PurchaseResponse Map(Purchase purchase) => new(
        purchase.PurchaseId, purchase.SupplierId, purchase.Supplier?.Name ?? string.Empty, purchase.PurchaseDate, purchase.Status.ToString(), purchase.TotalAmount,
        purchase.Items.Select(x => new PurchaseItemResponse(x.PurchaseItemId, x.ProductId, x.Product?.Name ?? string.Empty, x.Quantity, x.ReceivedQuantity, Math.Max(0, x.Quantity - x.ReceivedQuantity), x.UnitPrice, x.BatchId)).ToList());
}
