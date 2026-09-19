using StoreManagement.Domain.Entities;
namespace StoreManagement.Application.Interfaces.Data;
public interface IPurchaseDataService
{
    Task<Purchase?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Purchase purchase, CancellationToken cancellationToken);
    void Update(Purchase purchase);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
