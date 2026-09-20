using StoreManagement.Application.DTOs;
namespace StoreManagement.Application.Interfaces;
public interface IReportService
{
    Task<ReportResponse> GetAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken);
}
