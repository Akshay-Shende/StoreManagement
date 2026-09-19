namespace StoreManagement.Application.DTOs;

public record CreatePurchaseItemRequest(Guid ProductId, decimal Quantity, decimal UnitPrice);

public record CreatePurchaseRequest(
    Guid SupplierId,
    DateTimeOffset? PurchaseDate,
    IReadOnlyCollection<CreatePurchaseItemRequest> Items);

public record ReceivePurchaseItemRequest(
    Guid PurchaseItemId,
    decimal ReceivedQuantity,
    string? BatchNumber,
    DateOnly? ManufacturingDate,
    DateOnly? ExpiryDate);

public record ReceivePurchaseRequest(IReadOnlyCollection<ReceivePurchaseItemRequest> Items);

public record PurchaseResponse(
    Guid PurchaseId,
    Guid SupplierId,
    string SupplierName,
    DateTimeOffset PurchaseDate,
    string Status,
    decimal TotalAmount,
    IReadOnlyCollection<PurchaseItemResponse> Items);

public record PurchaseItemResponse(
    Guid PurchaseItemId,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    Guid? BatchId);
