using StoreManagement.Domain.Entities;
namespace StoreManagement.Application.Interfaces.Data;
public interface ICategoryDataService
{
    IQueryable<Category> Query();
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
