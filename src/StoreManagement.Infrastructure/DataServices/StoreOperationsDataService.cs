using System.Data;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class StoreOperationsDataService(StoreDbContext db) : IStoreOperationsDataService
{
    public IQueryable<Sale> QuerySales() => db.Sales.Include(x => x.Items).ThenInclude(x => x.Product).AsNoTracking();
    public IQueryable<SaleItem> QuerySaleItems() => db.SaleItems.Include(x => x.Product);
    public IQueryable<Payment> QueryPayments() => db.Payments.AsNoTracking();
    public IQueryable<Return> QueryReturns() => db.Returns.Include(x => x.Items).AsNoTracking();
    public IQueryable<GoodsReceipt> QueryGoodsReceipts() => db.GoodsReceipts.AsNoTracking();
    public IQueryable<AuditLog> QueryAuditLogs() => db.AuditLogs.AsNoTracking();
    public IQueryable<Notification> QueryNotifications() => db.Notifications.AsNoTracking();
    public Task<GoodsReceipt?> GetGoodsReceiptAsync(long id, CancellationToken cancellationToken) => db.GoodsReceipts.Include(x => x.Items).ThenInclude(x => x.Batches).FirstOrDefaultAsync(x => x.GoodsReceiptId == id, cancellationToken);
    public async Task AddGoodsReceiptAsync(GoodsReceipt receipt, CancellationToken cancellationToken) => await db.GoodsReceipts.AddAsync(receipt, cancellationToken);
    public async Task AddSaleItemBatchAsync(SaleItemBatch allocation, CancellationToken cancellationToken) => await db.SaleItemBatches.AddAsync(allocation, cancellationToken);
    public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken) => await db.Payments.AddAsync(payment, cancellationToken);
    public async Task AddReturnAsync(Return returnRecord, CancellationToken cancellationToken) => await db.Returns.AddAsync(returnRecord, cancellationToken);
    public async Task AddAuditAsync(AuditLog audit, CancellationToken cancellationToken) => await db.AuditLogs.AddAsync(audit, cancellationToken);
    public async Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken) => await db.Notifications.AddAsync(notification, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try { await operation(cancellationToken); await db.SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken); }
        catch { await tx.RollbackAsync(CancellationToken.None); throw; }
    }
}
