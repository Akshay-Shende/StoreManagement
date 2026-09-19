using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponse> CreateAndCompleteAsync(CreateSaleRequest request, string createdBy, CancellationToken cancellationToken);
    Task<SaleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
