using StoreManagement.Domain.Entities;
namespace StoreManagement.Application.Interfaces.Data;
public interface ISupplierDataService
{
    IQueryable<Supplier> Query();
    Task<Supplier?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
