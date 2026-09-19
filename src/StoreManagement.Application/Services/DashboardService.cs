using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;

namespace StoreManagement.Application.Services;

public sealed class DashboardService(IDashboardDataService data) : IDashboardService
{
    public async Task<DashboardResponse> GetAsync(CancellationToken cancellationToken) =>
        new(
            await data.CountActiveProductsAsync(cancellationToken),
            await data.GetCurrentInventoryUnitsAsync(cancellationToken),
            await data.CountLowStockProductsAsync(cancellationToken),
            await data.CountExpiringBatchesAsync(30, cancellationToken),
            await data.CountExpiredBatchesAsync(cancellationToken),
            await data.GetTodayGrossSalesAsync(cancellationToken));
}
