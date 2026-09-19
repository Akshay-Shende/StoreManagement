using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
}
