namespace StoreManagement.Application.DTOs;

public record CreateSaleItemRequest(long ProductId, decimal Quantity, decimal? UnitPrice);
public record CreateSaleRequest(long? CustomerId, IReadOnlyCollection<CreateSaleItemRequest> Items);
public record SaleResponse(long SaleId, DateTimeOffset SaleDate, long? CustomerId, decimal TotalAmount, string Status, string PaymentStatus, decimal PaidAmount, decimal DueAmount, IReadOnlyCollection<SaleItemResponse> Items);
public record SaleItemBatchResponse(long BatchId, string BatchNumber, decimal Quantity, DateOnly? ExpiryDate);
public record SaleItemResponse(long SaleItemId, long ProductId, string ProductName, decimal Quantity, decimal UnitPrice, IReadOnlyCollection<SaleItemBatchResponse> Batches);
public record CreatePaymentRequest(decimal Amount, string Method, string? TransactionReference = null, string? ClientRequestId = null);
public record PaymentResponse(long PaymentId, long SaleId, decimal Amount, string Method, string Status, string? TransactionReference, DateTimeOffset PaidAt, string CreatedBy);
