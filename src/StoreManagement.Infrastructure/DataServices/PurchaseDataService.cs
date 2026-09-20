using System.Data;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class PurchaseDataService(StoreDbContext db) : IPurchaseDataService
{
    public async Task<IReadOnlyCollection<Purchase>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Purchases
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.PurchaseDate)
            .ToListAsync(cancellationToken);

    public Task<Purchase?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        db.Purchases
            .Include(x => x.Supplier)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.PurchaseId == id, cancellationToken);

    public async Task AddAsync(Purchase purchase, CancellationToken cancellationToken) =>
        await db.Purchases.AddAsync(purchase, cancellationToken);

    public void Update(Purchase purchase) => db.Purchases.Update(purchase);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

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
}
