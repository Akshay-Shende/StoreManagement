using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;
namespace StoreManagement.Infrastructure.DataServices;
public sealed class CustomerDataService(StoreDbContext db) : ICustomerDataService
{
    public IQueryable<Customer> Query() => db.Customers;
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken) => await db.Customers.AddAsync(customer, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
