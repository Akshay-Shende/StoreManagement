using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;
using StoreManagement.Infrastructure.Data;

namespace StoreManagement.Infrastructure.DataServices;

public sealed class ProductDataService(StoreDbContext db) : IProductDataService
{
    public IQueryable<Product> Query() => db.Products.Include(x => x.Category);

    public Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        Query().FirstOrDefaultAsync(x => x.ProductId == id, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken) =>
        await db.Products.AddAsync(product, cancellationToken);

    public void Update(Product product) => db.Products.Update(product);

    public Task<bool> CategoryExistsAsync(long categoryId, CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(x => x.CategoryId == categoryId && x.IsActive, cancellationToken);

    public Task<bool> SkuExistsAsync(string sku, long? excludingProductId, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(x => x.SKU == sku && (!excludingProductId.HasValue || x.ProductId != excludingProductId.Value), cancellationToken);

    public Task<bool> BarcodeExistsAsync(string barcode, long? excludingProductId, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(x => x.Barcode == barcode && (!excludingProductId.HasValue || x.ProductId != excludingProductId.Value), cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
