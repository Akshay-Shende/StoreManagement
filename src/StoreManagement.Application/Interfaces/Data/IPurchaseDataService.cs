using StoreManagement.Domain.Entities;
namespace StoreManagement.Application.Interfaces.Data;
public interface IPurchaseDataService
{
    Task<IReadOnlyCollection<Purchase>> GetAllAsync(CancellationToken cancellationToken);
    Task<Purchase?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(Purchase purchase, CancellationToken cancellationToken);
    void Update(Purchase purchase);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
