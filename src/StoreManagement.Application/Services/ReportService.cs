using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;

namespace StoreManagement.Application.Services;

public sealed class ReportService(IReportDataService data) : IReportService
{
    public async Task<ReportResponse> GetAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken)
    {
        var to = toUtc ?? DateTimeOffset.UtcNow;
        var from = fromUtc ?? to.AddDays(-1);
        if (from >= to) throw new ArgumentException("From must be earlier than To.");
        var x = await data.GetSummaryAsync(from, to, cancellationToken);
        return new ReportResponse(x.GrossSales, x.RefundedAmount, x.NetSales, x.SaleCount, x.PurchaseValue, x.CurrentInventoryValue, x.LowStockProducts, x.ExpiringBatches, x.ExpiredBatches, from, to);
    }
}
