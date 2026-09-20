using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Interfaces.Data;

public interface IProductDataService
{
    IQueryable<Product> Query();
    Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    void Update(Product product);
    Task<bool> CategoryExistsAsync(long categoryId, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string sku, long? excludingProductId, CancellationToken cancellationToken);
    Task<bool> BarcodeExistsAsync(string barcode, long? excludingProductId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
