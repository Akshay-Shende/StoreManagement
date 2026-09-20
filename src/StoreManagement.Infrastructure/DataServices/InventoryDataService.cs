using System.Data;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class InventoryDataService(StoreDbContext db) : IInventoryDataService
{
    public IQueryable<Product> QueryProducts() => db.Products;
    public IQueryable<Batch> QueryBatches() => db.Batches.Include(x => x.Product);
    public IQueryable<InventoryTransaction> QueryTransactions() => db.InventoryTransactions;
    public IQueryable<StockAdjustment> QueryAdjustments() => db.StockAdjustments.Include(x => x.Product);

    public Task<Product?> GetProductAsync(long productId, CancellationToken cancellationToken) =>
        db.Products.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

    public Task<Batch?> GetBatchAsync(long batchId, CancellationToken cancellationToken) =>
        db.Batches.FirstOrDefaultAsync(x => x.BatchId == batchId, cancellationToken);

    public async Task AddBatchAsync(Batch batch, CancellationToken cancellationToken) =>
        await db.Batches.AddAsync(batch, cancellationToken);

    public async Task AddTransactionAsync(InventoryTransaction transaction, CancellationToken cancellationToken) =>
        await db.InventoryTransactions.AddAsync(transaction, cancellationToken);

    public async Task AddAdjustmentAsync(StockAdjustment adjustment, CancellationToken cancellationToken) =>
        await db.StockAdjustments.AddAsync(adjustment, cancellationToken);

    public Task<StockAdjustment?> GetAdjustmentAsync(long adjustmentId, CancellationToken cancellationToken) =>
        QueryAdjustments().FirstOrDefaultAsync(x => x.AdjustmentId == adjustmentId, cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            await operation(cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
