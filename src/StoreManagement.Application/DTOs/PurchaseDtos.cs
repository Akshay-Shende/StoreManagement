namespace StoreManagement.Application.DTOs;

public record CreatePurchaseItemRequest(long ProductId, decimal Quantity, decimal UnitPrice);

public record CreatePurchaseRequest(
    long SupplierId,
    DateTimeOffset? PurchaseDate,
    IReadOnlyCollection<CreatePurchaseItemRequest> Items);

public record ReceivePurchaseItemRequest(
    long PurchaseItemId,
    decimal ReceivedQuantity,
    string? BatchNumber,
    DateOnly? ManufacturingDate,
    DateOnly? ExpiryDate);

public record ReceivePurchaseRequest(IReadOnlyCollection<ReceivePurchaseItemRequest> Items);

public record PurchaseResponse(
    long PurchaseId,
    long SupplierId,
    string SupplierName,
    DateTimeOffset PurchaseDate,
    string Status,
    decimal TotalAmount,
    IReadOnlyCollection<PurchaseItemResponse> Items);

public record PurchaseItemResponse(
    long PurchaseItemId,
    long ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    long? BatchId);
