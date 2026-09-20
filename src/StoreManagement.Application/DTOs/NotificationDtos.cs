namespace StoreManagement.Application.DTOs;
public record NotificationResponse(long NotificationId, string Type, string Title, string Message, string Severity, long? ProductId, long? BatchId, bool IsRead, DateTimeOffset CreatedAt);
public record PurchaseSuggestionResponse(long ProductId, string ProductName, decimal CurrentStock, decimal ReorderLevel, decimal SuggestedQuantity, string Reason);
