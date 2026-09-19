namespace StoreManagement.Application.DTOs;

public record InventoryResponse(
    Guid ProductId,
    string ProductName,
    string? Sku,
    string Unit,
    decimal CurrentStock,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    bool IsLowStock);

public record ExpiringBatchResponse(
    Guid BatchId,
    Guid ProductId,
    string ProductName,
    string BatchNumber,
    decimal AvailableQuantity,
    DateOnly? ExpiryDate,
    int DaysUntilExpiry);

public record StockAdjustmentResponse(
    Guid AdjustmentId,
    Guid ProductId,
    string ProductName,
    decimal SystemQuantity,
    decimal PhysicalQuantity,
    decimal Difference,
    string Reason,
    string? ApprovedBy,
    bool IsApproved,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ApprovedAt);

public record CreateStockAdjustmentRequest(
    Guid ProductId,
    decimal PhysicalQuantity,
    string Reason);

public record ApproveStockAdjustmentRequest(string ApprovedBy);

public record InventoryMovementRequest(
    Guid ProductId,
    Guid? BatchId,
    decimal Quantity,
    string Reason);

public record DashboardResponse(
    int TotalProducts,
    decimal CurrentInventoryUnits,
    int LowStockProducts,
    int ExpiringBatches,
    int ExpiredBatches,
    decimal TodayGrossSales);
