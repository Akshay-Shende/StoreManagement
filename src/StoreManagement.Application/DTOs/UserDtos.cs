using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.DTOs;
public record UserListResponse(long UserId, string Username, string Email, string FullName, string Role, bool IsActive, DateTime CreatedAt);
public record UpdateUserRequest(string FullName, string Email, UserRole Role, bool IsActive);
