namespace StoreManagement.Application.Interfaces.Data;
public interface IDashboardDataService
{
    Task<int> CountActiveProductsAsync(CancellationToken cancellationToken);
    Task<decimal> GetCurrentInventoryUnitsAsync(CancellationToken cancellationToken);
    Task<int> CountLowStockProductsAsync(CancellationToken cancellationToken);
    Task<int> CountExpiringBatchesAsync(int days, CancellationToken cancellationToken);
    Task<int> CountExpiredBatchesAsync(CancellationToken cancellationToken);
    Task<decimal> GetTodayGrossSalesAsync(CancellationToken cancellationToken);
}
