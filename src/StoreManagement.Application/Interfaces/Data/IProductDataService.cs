using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Interfaces.Data;

public interface IProductDataService
{
    IQueryable<Product> Query();
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    void Update(Product product);
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string sku, Guid? excludingProductId, CancellationToken cancellationToken);
    Task<bool> BarcodeExistsAsync(string barcode, Guid? excludingProductId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
