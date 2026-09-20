using System.Text.Json.Serialization;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.DTOs;
public sealed record RegisterRequest(string Username, string Email, string Password, string FullName, UserRole Role = UserRole.Staff);
public sealed record LoginRequest(string UsernameOrEmail, string Password);
public sealed record RefreshTokenRequest(string AccessToken, [property: JsonIgnore] string? RefreshToken = null);
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, UserInfoResponse User, [property: JsonIgnore] string RefreshToken);
public sealed record UserInfoResponse(long UserId, string Username, string Email, string FullName, string Role, bool IsActive = true);
public sealed record MessageResponse(string Message);
