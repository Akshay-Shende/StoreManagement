using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync(CancellationToken cancellationToken);
}
