using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Interfaces.Data;

public interface IStoreOperationsDataService
{
    IQueryable<Sale> QuerySales();
    IQueryable<SaleItem> QuerySaleItems();
    IQueryable<Payment> QueryPayments();
    IQueryable<Return> QueryReturns();
    IQueryable<GoodsReceipt> QueryGoodsReceipts();
    IQueryable<AuditLog> QueryAuditLogs();
    IQueryable<Notification> QueryNotifications();
    Task<GoodsReceipt?> GetGoodsReceiptAsync(long id, CancellationToken cancellationToken);
    Task AddGoodsReceiptAsync(GoodsReceipt receipt, CancellationToken cancellationToken);
    Task AddSaleItemBatchAsync(SaleItemBatch allocation, CancellationToken cancellationToken);
    Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken);
    Task AddReturnAsync(Return returnRecord, CancellationToken cancellationToken);
    Task AddAuditAsync(AuditLog audit, CancellationToken cancellationToken);
    Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
