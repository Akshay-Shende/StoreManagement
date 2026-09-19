using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface ISupplierService
{
    Task<IReadOnlyCollection<SupplierResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken);
}
