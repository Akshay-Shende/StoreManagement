using Microsoft.EntityFrameworkCore;
using StoreManagement.Application.DTOs;
using StoreManagement.Application.Interfaces;
using StoreManagement.Application.Interfaces.Data;
using StoreManagement.Domain.Entities;

namespace StoreManagement.Application.Services;

public sealed class CategoryService(ICategoryDataService data) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await data.Query().AsNoTracking().OrderBy(x => x.Name).Select(x => new CategoryResponse(x.CategoryId, x.Name, x.Description, x.IsActive)).ToListAsync(cancellationToken);

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Category name is required.");
        if (await data.Query().AnyAsync(x => x.Name == request.Name.Trim(), cancellationToken)) throw new InvalidOperationException("Category already exists.");
        var category = new Category { Name = request.Name.Trim(), Description = request.Description?.Trim(), IsActive = true };
        await data.AddAsync(category, cancellationToken);
        await data.SaveChangesAsync(cancellationToken);
        return new CategoryResponse(category.CategoryId, category.Name, category.Description, category.IsActive);
    }
}
