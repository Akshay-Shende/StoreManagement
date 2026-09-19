using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class SupplierDataService(StoreDbContext db) : ISupplierDataService
{
    public IQueryable<Supplier> Query() => db.Suppliers;
    public Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => db.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == id, cancellationToken);
    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken) => await db.Suppliers.AddAsync(supplier, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
