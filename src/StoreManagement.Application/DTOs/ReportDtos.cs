namespace StoreManagement.Application.DTOs;
public record ReportResponse(decimal GrossSales, decimal RefundedAmount, decimal NetSales, int SaleCount, decimal PurchaseValue, decimal CurrentInventoryValue, int LowStockProducts, int ExpiringBatches, int ExpiredBatches, DateTimeOffset FromUtc, DateTimeOffset ToUtc);
