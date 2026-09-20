namespace StoreManagement.Application.DTOs;

public record CreateSaleItemRequest(long ProductId, decimal Quantity, decimal? UnitPrice);

public record CreateSaleRequest(
    long? CustomerId,
    IReadOnlyCollection<CreateSaleItemRequest> Items);

public record SaleResponse(
    long SaleId,
    DateTimeOffset SaleDate,
    long? CustomerId,
    decimal TotalAmount,
    string Status,
    string PaymentStatus,
    IReadOnlyCollection<SaleItemResponse> Items);

public record SaleItemResponse(
    long SaleItemId,
    long ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice);
