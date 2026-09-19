using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Services;

public sealed class ProductService(IProductDataService data) : IProductService
{
    public async Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await data.Query().AsNoTracking().OrderBy(x => x.Name).Select(ToResponseExpression).ToListAsync(cancellationToken);

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await data.Query().AsNoTracking().Where(x => x.ProductId == id).Select(ToResponseExpression).FirstOrDefaultAsync(cancellationToken);

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        ValidateProduct(request.Name, request.PurchasePrice, request.SellingPrice, request.ReorderLevel, request.ReorderQuantity);
        if (!await data.CategoryExistsAsync(request.CategoryId, cancellationToken)) throw new KeyNotFoundException("Category was not found or is inactive.");
        if (!string.IsNullOrWhiteSpace(request.Sku) && await data.SkuExistsAsync(request.Sku.Trim(), null, cancellationToken)) throw new InvalidOperationException("SKU already exists.");
        if (!string.IsNullOrWhiteSpace(request.Barcode) && await data.BarcodeExistsAsync(request.Barcode.Trim(), null, cancellationToken)) throw new InvalidOperationException("Barcode already exists.");

        var product = new Product
        {
            Name = request.Name.Trim(), SKU = NullIfBlank(request.Sku), Barcode = NullIfBlank(request.Barcode), CategoryId = request.CategoryId,
            Unit = request.Unit.Trim(), PurchasePrice = request.PurchasePrice, SellingPrice = request.SellingPrice,
            ReorderLevel = request.ReorderLevel, ReorderQuantity = request.ReorderQuantity,
            RequiresBatchTracking = request.RequiresBatchTracking, CurrentStock = 0, IsActive = true
        };
        await data.AddAsync(product, cancellationToken);
        await data.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(product.ProductId, cancellationToken) ?? throw new InvalidOperationException("Product could not be reloaded.");
    }

    public async Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        ValidateProduct(request.Name, request.PurchasePrice, request.SellingPrice, request.ReorderLevel, request.ReorderQuantity);
        var product = await data.GetByIdAsync(id, cancellationToken);
        if (product is null) return null;
        if (!await data.CategoryExistsAsync(request.CategoryId, cancellationToken)) throw new KeyNotFoundException("Category was not found or is inactive.");
        if (!string.IsNullOrWhiteSpace(request.Sku) && await data.SkuExistsAsync(request.Sku.Trim(), id, cancellationToken)) throw new InvalidOperationException("SKU already exists.");
        if (!string.IsNullOrWhiteSpace(request.Barcode) && await data.BarcodeExistsAsync(request.Barcode.Trim(), id, cancellationToken)) throw new InvalidOperationException("Barcode already exists.");

        product.Name = request.Name.Trim(); product.SKU = NullIfBlank(request.Sku); product.Barcode = NullIfBlank(request.Barcode);
        product.CategoryId = request.CategoryId; product.Unit = request.Unit.Trim(); product.PurchasePrice = request.PurchasePrice;
        product.SellingPrice = request.SellingPrice; product.ReorderLevel = request.ReorderLevel; product.ReorderQuantity = request.ReorderQuantity;
        product.RequiresBatchTracking = request.RequiresBatchTracking; product.IsActive = request.IsActive;
        data.Update(product);
        await data.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private static void ValidateProduct(string name, decimal purchasePrice, decimal sellingPrice, decimal reorderLevel, decimal reorderQuantity)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name is required.");
        if (purchasePrice < 0 || sellingPrice < 0 || reorderLevel < 0 || reorderQuantity < 0) throw new ArgumentException("Price and reorder values cannot be negative.");
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static readonly Expression<Func<Product, ProductResponse>> ToResponseExpression = x => new ProductResponse(
        x.ProductId, x.Name, x.SKU, x.Barcode, x.CategoryId, x.Category.Name, x.Unit,
        x.PurchasePrice, x.SellingPrice, x.ReorderLevel, x.ReorderQuantity, x.CurrentStock, x.RequiresBatchTracking, x.IsActive);
}
