namespace StoreManagement.Application.DTOs;

public record CreateReturnItemRequest(long SaleItemId, decimal Quantity, long? BatchId = null);
public record CreateReturnRequest(long SaleId, string Reason, string Condition, IReadOnlyCollection<CreateReturnItemRequest> Items, string? ClientRequestId = null);
public record ReturnItemResponse(long ReturnItemId, long SaleItemId, long? BatchId, decimal Quantity, decimal RefundAmount);
public record ReturnResponse(long ReturnId, long SaleId, decimal RefundAmount, string Reason, string Condition, string Status, string CreatedBy, DateTimeOffset CreatedAt, IReadOnlyCollection<ReturnItemResponse> Items);
