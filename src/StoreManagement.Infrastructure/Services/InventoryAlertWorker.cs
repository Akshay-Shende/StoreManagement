using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Infrastructure.Services;

public sealed class InventoryAlertWorker(IServiceScopeFactory scopeFactory, ILogger<InventoryAlertWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await GenerateAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Inventory alert generation failed."); }
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task GenerateAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var inventory = scope.ServiceProvider.GetRequiredService<IInventoryDataService>();
        var operations = scope.ServiceProvider.GetRequiredService<IStoreOperationsDataService>();
        var recent = DateTimeOffset.UtcNow.AddHours(-24);
        var existing = await operations.QueryNotifications().Where(x => x.CreatedAt >= recent).Select(x => new { x.Type, x.ProductId, x.BatchId }).ToListAsync(cancellationToken);
        var notifications = new List<Notification>();
        var low = await inventory.QueryProducts().AsNoTracking().Where(x => x.IsActive && x.CurrentStock <= x.ReorderLevel).Select(x => new { x.ProductId, x.Name, x.CurrentStock, x.ReorderLevel }).ToListAsync(cancellationToken);
        foreach (var p in low.Where(x => !existing.Any(e => e.Type == "LowStock" && e.ProductId == x.ProductId))) notifications.Add(new Notification { Type = "LowStock", Severity = "Warning", Title = $"Low stock: {p.Name}", Message = $"Current stock {p.CurrentStock} is at or below reorder level {p.ReorderLevel}.", ProductId = p.ProductId });
        var today = DateOnly.FromDateTime(DateTime.UtcNow); var end = today.AddDays(30);
        var expiring = await inventory.QueryBatches().AsNoTracking().Where(x => x.IsSellable && x.AvailableQuantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value >= today && x.ExpiryDate.Value <= end).Select(x => new { x.BatchId, x.ProductId, ProductName = x.Product.Name, x.BatchNumber, x.ExpiryDate }).ToListAsync(cancellationToken);
        foreach (var b in expiring.Where(x => !existing.Any(e => e.Type == "Expiry" && e.BatchId == x.BatchId))) notifications.Add(new Notification { Type = "Expiry", Severity = "Warning", Title = $"Expiry alert: {b.ProductName}", Message = $"Batch {b.BatchNumber} expires on {b.ExpiryDate:yyyy-MM-dd}.", ProductId = b.ProductId, BatchId = b.BatchId });
        foreach (var n in notifications) await operations.AddNotificationAsync(n, cancellationToken);
        if (notifications.Count > 0) await operations.SaveChangesAsync(cancellationToken);
    }
}
