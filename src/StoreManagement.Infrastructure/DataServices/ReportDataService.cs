using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Enums;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class ReportDataService(StoreDbContext db) : IReportDataService
{
    public async Task<ReportSummary> GetSummaryAsync(DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken)
    {
        var gross = await db.Sales.Where(x => x.Status == SaleStatus.Completed && x.SaleDate >= fromUtc && x.SaleDate < toUtc).SumAsync(x => x.TotalAmount, cancellationToken);
        var refunded = await db.Returns.Where(x => x.CreatedAt >= fromUtc && x.CreatedAt < toUtc && x.Status == "Completed").SumAsync(x => x.RefundAmount, cancellationToken);
        var saleCount = await db.Sales.CountAsync(x => x.Status == SaleStatus.Completed && x.SaleDate >= fromUtc && x.SaleDate < toUtc, cancellationToken);
        var purchaseValue = await db.PurchaseItems.Where(x => x.Purchase.CreatedAt >= fromUtc && x.Purchase.CreatedAt < toUtc).Select(x => x.Quantity * x.UnitPrice).SumAsync(cancellationToken);
        var inventoryValue = await db.Products.Where(x => x.IsActive).Select(x => x.CurrentStock * x.PurchasePrice).SumAsync(cancellationToken);
        var low = await db.Products.CountAsync(x => x.IsActive && x.CurrentStock <= x.ReorderLevel, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = today.AddDays(30);
        var expiring = await db.Batches.CountAsync(x => x.IsSellable && x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value >= today && x.ExpiryDate.Value <= end, cancellationToken);
        var expired = await db.Batches.CountAsync(x => x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value < today, cancellationToken);
        return new ReportSummary(gross, refunded, gross - refunded, saleCount, purchaseValue, inventoryValue, low, expiring, expired);
    }
}
