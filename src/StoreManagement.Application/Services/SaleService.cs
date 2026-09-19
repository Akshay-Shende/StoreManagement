using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Application.Services;

public sealed class SaleService(
    ISaleDataService saleData,
    IInventoryDataService inventoryData) : ISaleService
{
    public async Task<SaleResponse> CreateAndCompleteAsync(CreateSaleRequest request, string createdBy, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0) throw new ArgumentException("At least one sale item is required.");

        var sale = new Sale
        {
            CustomerId = request.CustomerId,
            Status = SaleStatus.Completed,
            PaymentStatus = PaymentStatus.Pending,
            SaleDate = DateTimeOffset.UtcNow
        };

        await saleData.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var requestItem in request.Items)
            {
                if (requestItem.Quantity <= 0) throw new ArgumentException("Sale quantity must be greater than zero.");
                var product = await inventoryData.QueryProducts().FirstOrDefaultAsync(x => x.ProductId == requestItem.ProductId && x.IsActive, ct)
                    ?? throw new KeyNotFoundException($"Product {requestItem.ProductId} was not found or is inactive.");
                var unitPrice = requestItem.UnitPrice ?? product.SellingPrice;
                if (unitPrice < 0) throw new ArgumentException("Sale unit price cannot be negative.");
                if (product.CurrentStock < requestItem.Quantity) throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.CurrentStock}, requested: {requestItem.Quantity}.");

                var saleItem = new SaleItem { ProductId = product.ProductId, Product = product, Quantity = requestItem.Quantity, UnitPrice = unitPrice };
                sale.Items.Add(saleItem);
                product.CurrentStock -= requestItem.Quantity;

                if (product.RequiresBatchTracking)
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var remaining = requestItem.Quantity;
                    var batches = await inventoryData.QueryBatches()
                        .Where(x => x.ProductId == product.ProductId && x.IsSellable && x.AvailableQuantity > 0 && (!x.ExpiryDate.HasValue || x.ExpiryDate.Value >= today))
                        .OrderBy(x => x.ExpiryDate.HasValue ? 0 : 1)
                        .ThenBy(x => x.ExpiryDate)
                        .ThenBy(x => x.ReceivedAt)
                        .ToListAsync(ct);
                    if (batches.Sum(x => x.AvailableQuantity) < requestItem.Quantity) throw new InvalidOperationException($"Batch-level sellable stock is insufficient for {product.Name}.");

                    foreach (var batch in batches)
                    {
                        if (remaining <= 0) break;
                        var consumed = Math.Min(remaining, batch.AvailableQuantity);
                        batch.AvailableQuantity -= consumed;
                        remaining -= consumed;
                        await inventoryData.AddTransactionAsync(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            BatchId = batch.BatchId,
                            TransactionType = InventoryTransactionType.Sale,
                            QuantityDelta = -consumed,
                            ReferenceId = sale.SaleId,
                            Reason = "Customer sale - FEFO",
                            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy
                        }, ct);
                    }
                }
                else
                {
                    await inventoryData.AddTransactionAsync(new InventoryTransaction
                    {
                        ProductId = product.ProductId,
                        TransactionType = InventoryTransactionType.Sale,
                        QuantityDelta = -requestItem.Quantity,
                        ReferenceId = sale.SaleId,
                        Reason = "Customer sale",
                        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy
                    }, ct);
                }
            }

            sale.TotalAmount = sale.Items.Sum(x => x.Quantity * x.UnitPrice);
            await saleData.AddAsync(sale, ct);
        }, cancellationToken);

        return Map(sale);
    }

    public async Task<SaleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sale = await saleData.GetByIdAsync(id, cancellationToken);
        return sale is null ? null : Map(sale);
    }

    private static SaleResponse Map(Sale sale) => new(sale.SaleId, sale.SaleDate, sale.CustomerId, sale.TotalAmount, sale.Status.ToString(), sale.PaymentStatus.ToString(),
        sale.Items.Select(x => new SaleItemResponse(x.SaleItemId, x.ProductId, x.Product.Name, x.Quantity, x.UnitPrice)).ToList());
}
