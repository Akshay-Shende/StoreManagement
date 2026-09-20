namespace StoreManagement.Application.Interfaces.Data;

public interface IReportDataService
{
    Task<ReportSummary> GetSummaryAsync(DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
}

public sealed record ReportSummary(
    decimal GrossSales,
    decimal RefundedAmount,
    decimal NetSales,
    int SaleCount,
    decimal PurchaseValue,
    decimal CurrentInventoryValue,
    int LowStockProducts,
    int ExpiringBatches,
    int ExpiredBatches);
