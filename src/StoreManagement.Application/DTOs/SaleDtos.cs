namespace StoreManagement.Application.DTOs;

public record CreateSaleItemRequest(Guid ProductId, decimal Quantity, decimal? UnitPrice);

public record CreateSaleRequest(
    Guid? CustomerId,
    IReadOnlyCollection<CreateSaleItemRequest> Items);

public record SaleResponse(
    Guid SaleId,
    DateTimeOffset SaleDate,
    Guid? CustomerId,
    decimal TotalAmount,
    string Status,
    string PaymentStatus,
    IReadOnlyCollection<SaleItemResponse> Items);

public record SaleItemResponse(
    Guid SaleItemId,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice);
