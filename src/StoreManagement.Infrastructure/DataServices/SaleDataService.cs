using System.Data;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class SaleDataService(StoreDbContext db) : ISaleDataService
{
    public Task<Sale?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        db.Sales
            .Include(x => x.Customer)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.SaleId == id, cancellationToken);

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken) =>
        await db.Sales.AddAsync(sale, cancellationToken);

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
