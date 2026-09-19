using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class CategoryDataService(StoreDbContext db) : ICategoryDataService
{
    public IQueryable<Category> Query() => db.Categories;
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => db.Categories.FirstOrDefaultAsync(x => x.CategoryId == id, cancellationToken);
    public async Task AddAsync(Category category, CancellationToken cancellationToken) => await db.Categories.AddAsync(category, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
