using StoreManagement.Domain.Entities;
namespace StoreManagement.Application.Interfaces.Data;
public interface ISaleDataService
{
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Sale sale, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
