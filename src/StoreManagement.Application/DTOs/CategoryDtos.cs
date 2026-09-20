namespace StoreManagement.Application.DTOs;

public record CreateCategoryRequest(string Name, string? Description);
public record CategoryResponse(long CategoryId, string Name, string? Description, bool IsActive);
