using StoreManagement.Domain.Entities;
namespace StoreManagement.Application.Interfaces.Data;
public interface IInventoryDataService
{
    IQueryable<Product> QueryProducts();
    IQueryable<Batch> QueryBatches();
    IQueryable<InventoryTransaction> QueryTransactions();
    IQueryable<StockAdjustment> QueryAdjustments();
    Task<Product?> GetProductAsync(long productId, CancellationToken cancellationToken);
    Task<Batch?> GetBatchAsync(long batchId, CancellationToken cancellationToken);
    Task AddBatchAsync(Batch batch, CancellationToken cancellationToken);
    Task AddTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken);
    Task AddAdjustmentAsync(StockAdjustment adjustment, CancellationToken cancellationToken);
    Task<StockAdjustment?> GetAdjustmentAsync(long adjustmentId, CancellationToken cancellationToken);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
