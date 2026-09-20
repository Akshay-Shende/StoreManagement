using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public sealed class SaleService(
    ISaleDataService saleData,
    IInventoryDataService inventoryData,
    IStoreOperationsDataService operations,
    IAuditService audit) : ISaleService
{
    public async Task<SaleResponse> CreateAndCompleteAsync(CreateSaleRequest request, string createdBy, string? clientRequestId, CancellationToken cancellationToken)
        => await CreateInternalAsync(request, createdBy, clientRequestId, allowPriceOverride: true, cancellationToken);

    public async Task<SaleResponse> CreateInternalAsync(CreateSaleRequest request, string createdBy, string? clientRequestId, bool allowPriceOverride, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0) throw new ArgumentException("At least one sale item is required.");
        if (!string.IsNullOrWhiteSpace(clientRequestId))
        {
            var existing = await operations.QuerySales().FirstOrDefaultAsync(x => x.ClientRequestId == clientRequestId, cancellationToken);
            if (existing is not null) return await MapAsync(existing, cancellationToken);
        }
        var sale = new Sale { CustomerId = request.CustomerId, Status = SaleStatus.Draft, PaymentStatus = PaymentStatus.Pending, SaleDate = DateTimeOffset.UtcNow, ClientRequestId = clientRequestId?.Trim() };
        await saleData.ExecuteInTransactionAsync(async ct =>
        {
            await saleData.AddAsync(sale, ct);
            await saleData.SaveChangesAsync(ct);
            foreach (var requestItem in request.Items)
            {
                if (requestItem.Quantity <= 0) throw new ArgumentException("Sale quantity must be greater than zero.");
                var product = await inventoryData.QueryProducts().FirstOrDefaultAsync(x => x.ProductId == requestItem.ProductId && x.IsActive, ct) ?? throw new KeyNotFoundException($"Product {requestItem.ProductId} was not found or is inactive.");
                var unitPrice = requestItem.UnitPrice ?? product.SellingPrice;
                if (unitPrice < 0) throw new ArgumentException("Sale unit price cannot be negative.");
                if (!allowPriceOverride && requestItem.UnitPrice.HasValue && requestItem.UnitPrice.Value != product.SellingPrice) throw new UnauthorizedAccessException("Your role cannot override the configured selling price.");
                if (product.CurrentStock < requestItem.Quantity) throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.CurrentStock}, requested: {requestItem.Quantity}.");

                var saleItem = new SaleItem { ProductId = product.ProductId, Product = product, Quantity = requestItem.Quantity, UnitPrice = unitPrice };
                sale.Items.Add(saleItem);
                product.CurrentStock -= requestItem.Quantity;
                if (product.RequiresBatchTracking)
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var batches = await inventoryData.QueryBatches().Where(x => x.ProductId == product.ProductId && x.IsSellable && x.AvailableQuantity > 0 && (!x.ExpiryDate.HasValue || x.ExpiryDate.Value >= today)).OrderBy(x => x.ExpiryDate.HasValue ? 0 : 1).ThenBy(x => x.ExpiryDate).ThenBy(x => x.ReceivedAt).ToListAsync(ct);
                    if (batches.Sum(x => x.AvailableQuantity) < requestItem.Quantity) throw new InvalidOperationException($"Batch-level sellable stock is insufficient for {product.Name}.");
                    var remaining = requestItem.Quantity;
                    foreach (var batch in batches)
                    {
                        if (remaining <= 0) break;
                        var consumed = Math.Min(remaining, batch.AvailableQuantity);
                        batch.AvailableQuantity -= consumed;
                        remaining -= consumed;
                        saleItem.BatchAllocations.Add(new SaleItemBatch { BatchId = batch.BatchId, Batch = batch, Quantity = consumed, UnitCost = product.PurchasePrice });
                        await inventoryData.AddTransactionAsync(new InventoryTransaction { ProductId = product.ProductId, BatchId = batch.BatchId, TransactionType = InventoryTransactionType.Sale, QuantityDelta = -consumed, ReferenceType = "Sale", ReferenceId = sale.SaleId, Reason = "Reserved for sale - FEFO", CreatedBy = createdBy }, ct);
                    }
                }
                else
                {
                    await inventoryData.AddTransactionAsync(new InventoryTransaction { ProductId = product.ProductId, TransactionType = InventoryTransactionType.Sale, QuantityDelta = -requestItem.Quantity, ReferenceType = "Sale", ReferenceId = sale.SaleId, Reason = "Reserved for sale", CreatedBy = createdBy }, ct);
                }
            }
            sale.TotalAmount = sale.Items.Sum(x => x.Quantity * x.UnitPrice);
        }, cancellationToken);
        await audit.WriteAsync("Created", "Sale", sale.SaleId.ToString(), createdBy, null, null, null, new { sale.SaleId, sale.TotalAmount, sale.PaymentStatus }, "Stock reserved; payment pending", cancellationToken);
        return await MapAsync(sale, cancellationToken);
    }

    public async Task<SaleResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var sale = await saleData.GetByIdAsync(id, cancellationToken);
        return sale is null ? null : await MapAsync(sale, cancellationToken);
    }

    public async Task<PaymentResponse?> AddPaymentAsync(long saleId, CreatePaymentRequest request, string createdBy, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0) throw new ArgumentException("Payment amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Method)) throw new ArgumentException("Payment method is required.");
        var sale = await saleData.GetByIdAsync(saleId, cancellationToken) ?? throw new KeyNotFoundException("Sale was not found.");
        if (sale.Status == SaleStatus.Cancelled) throw new InvalidOperationException("Cancelled sales cannot receive payments.");
        if (!string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            var duplicate = await operations.QueryPayments().FirstOrDefaultAsync(x => x.ClientRequestId == request.ClientRequestId, cancellationToken);
            if (duplicate is not null) return new PaymentResponse(duplicate.PaymentId, duplicate.SaleId, duplicate.Amount, duplicate.Method, duplicate.Status, duplicate.TransactionReference, duplicate.PaidAt, duplicate.CreatedBy);
        }
        var payment = new Payment { SaleId = saleId, Amount = request.Amount, Method = request.Method.Trim(), Status = "Completed", TransactionReference = request.TransactionReference?.Trim(), ClientRequestId = request.ClientRequestId?.Trim(), PaidAt = DateTimeOffset.UtcNow, CreatedBy = createdBy };
        decimal paid = 0;
        await operations.ExecuteInTransactionAsync(async ct =>
        {
            var currentSale = await saleData.GetByIdAsync(saleId, ct) ?? throw new KeyNotFoundException("Sale was not found.");
            if (currentSale.Status == SaleStatus.Cancelled) throw new InvalidOperationException("Cancelled sales cannot receive payments.");
            paid = await operations.QueryPayments().Where(x => x.SaleId == saleId && x.Status == "Completed").SumAsync(x => x.Amount, ct);
            var due = Math.Max(0, currentSale.TotalAmount - paid);
            if (request.Amount > due) throw new InvalidOperationException($"Payment exceeds the outstanding amount of {due:N2}.");
            await operations.AddPaymentAsync(payment, ct);
            paid += request.Amount;
            currentSale.PaymentStatus = paid >= currentSale.TotalAmount ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
            if (currentSale.PaymentStatus == PaymentStatus.Paid) currentSale.Status = SaleStatus.Completed;
            await saleData.SaveChangesAsync(ct);
        }, cancellationToken);
        await audit.WriteAsync("Payment", "Sale", saleId.ToString(), createdBy, null, null, new { Paid = paid - request.Amount }, new { Paid = paid, sale.PaymentStatus, sale.Status }, request.Method, cancellationToken);
        return new PaymentResponse(payment.PaymentId, payment.SaleId, payment.Amount, payment.Method, payment.Status, payment.TransactionReference, payment.PaidAt, payment.CreatedBy);
    }

    public async Task<SaleResponse?> CancelAsync(long saleId, string createdBy, CancellationToken cancellationToken)
    {
        var sale = await saleData.GetByIdAsync(saleId, cancellationToken);
        if (sale is null) return null;
        if (sale.Status == SaleStatus.Cancelled) throw new InvalidOperationException("Sale is already cancelled.");
        if (sale.Status == SaleStatus.Returned) throw new InvalidOperationException("Returned sales cannot be cancelled.");
        await saleData.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var item in sale.Items)
            {
                item.Product.CurrentStock += item.Quantity;
                if (item.BatchAllocations.Count > 0)
                {
                    foreach (var allocation in item.BatchAllocations)
                    {
                        allocation.Batch.AvailableQuantity += allocation.Quantity;
                        await inventoryData.AddTransactionAsync(new InventoryTransaction { ProductId = item.ProductId, BatchId = allocation.BatchId, TransactionType = InventoryTransactionType.Return, QuantityDelta = allocation.Quantity, ReferenceType = "SaleCancellation", ReferenceId = sale.SaleId, Reason = "Cancelled sale stock release", CreatedBy = createdBy }, ct);
                    }
                }
                else
                    await inventoryData.AddTransactionAsync(new InventoryTransaction { ProductId = item.ProductId, TransactionType = InventoryTransactionType.Return, QuantityDelta = item.Quantity, ReferenceType = "SaleCancellation", ReferenceId = sale.SaleId, Reason = "Cancelled sale stock release", CreatedBy = createdBy }, ct);
            }
            sale.Status = SaleStatus.Cancelled;
            sale.PaymentStatus = PaymentStatus.Refunded;
            await saleData.SaveChangesAsync(ct);
        }, cancellationToken);
        await audit.WriteAsync("Cancelled", "Sale", saleId.ToString(), createdBy, null, null, new { Status = "Active" }, new { sale.Status }, "Sale cancelled and stock released", cancellationToken);
        return await MapAsync(sale, cancellationToken);
    }

    private async Task<SaleResponse> MapAsync(Sale sale, CancellationToken cancellationToken)
    {
        var paid = await operations.QueryPayments().Where(x => x.SaleId == sale.SaleId && x.Status == "Completed").SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        return new SaleResponse(sale.SaleId, sale.SaleDate, sale.CustomerId, sale.TotalAmount, sale.Status.ToString(), sale.PaymentStatus.ToString(), paid, Math.Max(0, sale.TotalAmount - paid), sale.Items.Select(x => new SaleItemResponse(x.SaleItemId, x.ProductId, x.Product?.Name ?? string.Empty, x.Quantity, x.UnitPrice, x.BatchAllocations.Select(b => new SaleItemBatchResponse(b.BatchId, b.Batch?.BatchNumber ?? string.Empty, b.Quantity, b.Batch?.ExpiryDate)).ToList())).ToList());
    }

}
