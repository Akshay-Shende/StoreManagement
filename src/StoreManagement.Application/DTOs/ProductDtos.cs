namespace StoreManagement.Application.DTOs;

public record CreateProductRequest(
    string Name,
    string? Sku,
    string? Barcode,
    Guid CategoryId,
    string Unit,
    decimal PurchasePrice,
    decimal SellingPrice,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    bool RequiresBatchTracking);

public record UpdateProductRequest(
    string Name,
    string? Sku,
    string? Barcode,
    Guid CategoryId,
    string Unit,
    decimal PurchasePrice,
    decimal SellingPrice,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    bool RequiresBatchTracking,
    bool IsActive);

public record ProductResponse(
    Guid ProductId,
    string Name,
    string? Sku,
    string? Barcode,
    Guid CategoryId,
    string CategoryName,
    string Unit,
    decimal PurchasePrice,
    decimal SellingPrice,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    decimal CurrentStock,
    bool RequiresBatchTracking,
    bool IsActive);
