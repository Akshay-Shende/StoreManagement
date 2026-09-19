using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
}
