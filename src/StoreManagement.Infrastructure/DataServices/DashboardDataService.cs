using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Infrastructure.Data;
using StoreManagement.Domain.Enums;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class DashboardDataService(StoreDbContext db) : IDashboardDataService
{
    public Task<int> CountActiveProductsAsync(CancellationToken cancellationToken) =>
        db.Products.CountAsync(x => x.IsActive, cancellationToken);

    public Task<decimal> GetCurrentInventoryUnitsAsync(CancellationToken cancellationToken) =>
        db.Products.Where(x => x.IsActive).SumAsync(x => x.CurrentStock, cancellationToken);

    public Task<int> CountLowStockProductsAsync(CancellationToken cancellationToken) =>
        db.Products.CountAsync(x => x.IsActive && x.CurrentStock <= x.ReorderLevel, cancellationToken);

    public Task<int> CountExpiringBatchesAsync(int days, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = today.AddDays(days);
        return db.Batches.CountAsync(x => x.IsSellable && x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value >= today && x.ExpiryDate.Value <= end, cancellationToken);
    }

    public Task<int> CountExpiredBatchesAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return db.Batches.CountAsync(x => x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value < today, cancellationToken);
    }

    public Task<decimal> GetTodayGrossSalesAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var start = new DateTimeOffset(today, TimeSpan.Zero);
        var end = start.AddDays(1);
        return db.Sales.Where(x => x.Status == SaleStatus.Completed && x.SaleDate >= start && x.SaleDate < end).SumAsync(x => x.TotalAmount, cancellationToken);
    }
}
