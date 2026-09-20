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
    IInventoryDataService inventoryData) : IPurchaseService
{
    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0) throw new ArgumentException("At least one purchase item is required.");
        if (await supplierData.GetByIdAsync(request.SupplierId, cancellationToken) is null) throw new KeyNotFoundException("Supplier was not found.");

        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToArray();
        var products = await productData.Query().Where(x => productIds.Contains(x.ProductId) && x.IsActive).ToDictionaryAsync(x => x.ProductId, cancellationToken);
        if (products.Count != productIds.Length) throw new KeyNotFoundException("One or more products were not found or are inactive.");

        var purchase = new Purchase
        {
            SupplierId = request.SupplierId,
            PurchaseDate = request.PurchaseDate ?? DateTimeOffset.UtcNow,
            Status = PurchaseStatus.Draft,
            Items = request.Items.Select(x => new PurchaseItem
            {
                ProductId = x.ProductId,
                Quantity = ValidatePositive(x.Quantity, "Purchase quantity"),
                UnitPrice = ValidateNonNegative(x.UnitPrice, "Unit price")
            }).ToList()
        };
        purchase.TotalAmount = purchase.Items.Sum(x => x.Quantity * x.UnitPrice);

        await purchaseData.AddAsync(purchase, cancellationToken);
        await purchaseData.SaveChangesAsync(cancellationToken);
        return Map(purchase, products);
    }

    public async Task<PurchaseResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var purchase = await purchaseData.GetByIdAsync(id, cancellationToken);
        return purchase is null ? null : Map(purchase, purchase.Items.ToDictionary(x => x.ProductId, x => x.Product));
    }

    public async Task<PurchaseResponse?> ReceiveAsync(long id, ReceivePurchaseRequest request, string createdBy, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0) throw new ArgumentException("At least one receipt item is required.");
        var purchase = await purchaseData.GetByIdAsync(id, cancellationToken);
        if (purchase is null) return null;
        if (purchase.Status == PurchaseStatus.Cancelled || purchase.Status == PurchaseStatus.Received) throw new InvalidOperationException("Purchase cannot be received in its current status.");

        await purchaseData.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var receipt in request.Items)
            {
                var item = purchase.Items.FirstOrDefault(x => x.PurchaseItemId == receipt.PurchaseItemId) ?? throw new KeyNotFoundException($"Purchase item {receipt.PurchaseItemId} was not found.");
                var remaining = item.Quantity - item.ReceivedQuantity;
                var received = ValidatePositive(receipt.ReceivedQuantity, "Received quantity");
                if (received > remaining) throw new InvalidOperationException($"Received quantity exceeds the remaining quantity for purchase item {item.PurchaseItemId}.");

                var product = item.Product;
                Batch? batch = null;
                if (product.RequiresBatchTracking)
                {
                    if (string.IsNullOrWhiteSpace(receipt.BatchNumber)) throw new ArgumentException($"Batch number is required for {product.Name}.");
                    batch = await inventoryData.QueryBatches().FirstOrDefaultAsync(x => x.ProductId == product.ProductId && x.BatchNumber == receipt.BatchNumber.Trim(), ct);
                    if (batch is null)
                    {
                        batch = new Batch
                        {
                            ProductId = product.ProductId,
                            BatchNumber = receipt.BatchNumber.Trim(),
                            ReceivedQuantity = 0,
                            AvailableQuantity = 0,
                            ManufacturingDate = receipt.ManufacturingDate,
                            ExpiryDate = receipt.ExpiryDate,
                            IsSellable = true
                        };
                        await inventoryData.AddBatchAsync(batch, ct);
                    }
                    else if (receipt.ExpiryDate.HasValue && batch.ExpiryDate.HasValue && receipt.ExpiryDate.Value != batch.ExpiryDate.Value)
                    {
                        throw new InvalidOperationException("Expiry date does not match the existing batch.");
                    }
                    batch.ReceivedQuantity += received;
                    batch.AvailableQuantity += received;
                    batch.ManufacturingDate ??= receipt.ManufacturingDate;
                    batch.ExpiryDate ??= receipt.ExpiryDate;
                    item.BatchId = batch.BatchId;
                }

                item.ReceivedQuantity += received;
                product.CurrentStock += received;
                await inventoryData.AddTransactionAsync(new InventoryTransaction
                {
                    ProductId = product.ProductId,
                    BatchId = batch?.BatchId,
                    TransactionType = InventoryTransactionType.Purchase,
                    QuantityDelta = received,
                    ReferenceId = purchase.PurchaseId,
                    Reason = "Goods received",
                    CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy
                }, ct);
            }

            purchase.Status = purchase.Items.All(x => x.ReceivedQuantity >= x.Quantity) ? PurchaseStatus.Received : PurchaseStatus.PartiallyReceived;
            purchaseData.Update(purchase);
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private static PurchaseResponse Map(Purchase purchase, IReadOnlyDictionary<long, Product> products) =>
        new(purchase.PurchaseId, purchase.SupplierId, purchase.Supplier.Name, purchase.PurchaseDate, purchase.Status.ToString(), purchase.TotalAmount,
            purchase.Items.Select(x => new PurchaseItemResponse(x.PurchaseItemId, x.ProductId, products[x.ProductId].Name, x.Quantity, x.UnitPrice, x.BatchId)).ToList());

    private static decimal ValidatePositive(decimal value, string label)
    {
        if (value <= 0) throw new ArgumentException($"{label} must be greater than zero.");
        return value;
    }

    private static decimal ValidateNonNegative(decimal value, string label)
    {
        if (value < 0) throw new ArgumentException($"{label} cannot be negative.");
        return value;
    }
}
