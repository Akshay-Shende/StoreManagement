using StoreManagement.Domain.Entities;
namespace StoreManagement.Application.Interfaces.Data;
public interface ICustomerDataService
{
    IQueryable<Customer> Query();
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
